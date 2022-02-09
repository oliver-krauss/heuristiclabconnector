/*
 *
 *  * Copyright (c) 2022 the original author or authors.
 *  * DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *  *
 *  * This Source Code Form is subject to the terms of the Mozilla Public
 *  * License, v. 2.0. If a copy of the MPL was not distributed with this
 *  * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 *
 */

package at.fh.hagenberg.aist.hlc.broker;

import at.fh.hagenberg.aist.hlc.core.ParanoidPirateProtocolConstants;
import at.fh.hagenberg.aist.hlc.core.messages.WorkerConfiguration;
import at.fh.hagenberg.aist.seshat.Logger;
import com.google.protobuf.InvalidProtocolBufferException;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import org.zeromq.ZContext;
import org.zeromq.ZFrame;
import org.zeromq.ZMQ;
import org.zeromq.ZMQ.Poller;
import org.zeromq.ZMQ.Socket;
import org.zeromq.ZMsg;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.stream.Collectors;

/**
 * Broker that implements the Paranoid Pirate Pattern, based on http://zguide.zeromq.org/java:ppqueue.
 *
 * @author Daniel Dorfmeister on 2019-06-19
 */
public class Broker {
    private int heartbeatInterval;
    private int heartbeatLiveness;
    private String frontend;
    private String backend;
    private Logger logger;

    public static void main(String[] args) {
        ClassPathXmlApplicationContext configCtx = new ClassPathXmlApplicationContext("config.xml");
        Broker broker = configCtx.getBean("broker", Broker.class);
        broker.broker();
    }

    /**
     * The main task is an LRU queue with heartbeating on workers so we can detect crashed or blocked worker tasks
     */
    public void broker() {
        try (ZContext ctx = new ZContext()) {
            Socket frontendSocket = ctx.createSocket(ZMQ.ROUTER);
            Socket backendSocket = ctx.createSocket(ZMQ.ROUTER);
            frontendSocket.bind(frontend); //  For clients
            backendSocket.bind(backend); //  For workers

            //  Queue of available workers
            WorkerQueue queue = new WorkerQueue();

            //  All workers registered to the broker
            Map<byte[], Worker> registeredWorkers = new HashMap<>();
            //  Run ID to language
            Map<String, Long> requiredLanguages = new HashMap<>();

            //  Send out heartbeats at regular intervals
            long heartbeatAt = System.currentTimeMillis() + heartbeatInterval;

            Poller poller = ctx.createPoller(2);
            poller.register(backendSocket, Poller.POLLIN);
            poller.register(frontendSocket, Poller.POLLIN);

            while (true) {
                boolean workersAvailable = queue.size() > 0;
                int rc = poller.poll(heartbeatInterval);
                if (rc == -1) {
                    logger.error("poller not ready");
                    break; //  Interrupted
                }

                //  Handle worker activity on backend
                if (poller.pollin(0)) {
                    //  Use worker address for LRU routing
                    ZMsg msg = ZMsg.recvMsg(backendSocket);
                    if (msg == null) {
                        logger.error("message was null");
                        break; //  Interrupted
                    }

                    //  Any sign of life from worker means it's ready
                    ZFrame address = msg.unwrap();
                    byte[] id = address.getData();

                    //  Validate control message, or return reply to client
                    if (msg.size() == 1) { // signal
                        ZFrame frame = msg.getFirst();
                        String data = new String(frame.getData(), ZMQ.CHARSET);

                        if (data.equals(ParanoidPirateProtocolConstants.PPP_READY)) {
                            //  Read configuration of worker (not part of PPP)
                            ZMsg configMsg = ZMsg.recvMsg(backendSocket);
                            configMsg.pop();
                            ZFrame configFrame = configMsg.pop();
                            WorkerConfiguration config = WorkerConfiguration.parseFrom(configFrame.getData());
                            Worker worker = new Worker(address, config.getSupportedLanguagesList(),
                                    config.getIsConfigWorker(), heartbeatInterval, heartbeatLiveness);
                            registeredWorkers.put(id, worker);
                            logger.info("registered a new worker: " + worker);
                            configMsg.destroy();
                        } else if (data.equals(ParanoidPirateProtocolConstants.PPP_HEARTBEAT)) {
                            if (registeredWorkers.containsKey(id)) {
                                registeredWorkers.get(id).resetExpiry();
                            } else {
                                logger.error("received heartbeat from unregistered worker");
                                id = null;
                            }
                        } else {
                            logger.error("invalid message from worker: " + msg);
                            id = null;
                        }

                        msg.destroy();
                    } else {
                        logger.info("forward message to frontend");
                        msg.send(frontendSocket);
                    }

                    if (id != null) {
                        queue.push(registeredWorkers.get(id));
                    }
                }

                //  Now get next client request, route to next worker
                if (poller.pollin(1)) {
                    // consume the msg even if we don't have a worker to handle it to avoid an endless loop
                    ZMsg msg = ZMsg.recvMsg(frontendSocket);
                    if (msg == null) {
                        logger.error("message was null");
                        break; //  Interrupted
                    }

                    if (!workersAvailable) {
                        logger.error("no workers available to handle request");
                        continue;
                    }

                    if (msg.size() >= 4) { // frames: ZMQ ID, empty, algorithm run ID, [language ID], message
                        Object[] frames = msg.toArray();
                        String id = frames[2].toString();

                        //  First message must include required worker configuration before actual message
                        if (!requiredLanguages.containsKey(id)) {
                            long lang = Long.parseLong(frames[3].toString());
                            requiredLanguages.put(id, lang);
                            msg.remove(frames[3]);
                        }

                        long lang = requiredLanguages.get(id);

                        try {
                            msg.push(queue.pop(lang));
                            logger.info("forward message to worker");
                            msg.send(backendSocket);
                        } catch (IllegalStateException e) {
                            logger.error(e.getMessage());
                        }
                    } else { // frames: ZMQ ID, empty, message
                        try {
                            msg.push(queue.pop(true));
                            logger.info("forward message to worker");
                            msg.send(backendSocket);
                        } catch (IllegalStateException e) {
                            logger.error(e.getMessage());
                        }
                    }
                }

                //  We handle heartbeating after any socket activity. First we
                //  send heartbeats to any idle workers if it's time. Then we
                //  purge any dead workers
                if (System.currentTimeMillis() >= heartbeatAt) {
                    for (Worker worker : queue) {
                        worker.getAddress().send(backendSocket, ZFrame.REUSE + ZFrame.MORE);
                        ZFrame frame = new ZFrame(ParanoidPirateProtocolConstants.PPP_HEARTBEAT);
                        frame.send(backendSocket, 0);
                    }
                    heartbeatAt += heartbeatInterval;
                }

                logger.info("purge inactive workers");
                List<Worker> purgedWorkers = queue.purge();
                if (!purgedWorkers.isEmpty()) {
                    logger.info(String.format("purged worker(s): " +
                            purgedWorkers.stream().map(Worker::toString).collect(Collectors.joining(", "))));
                }
            }

            //  When we're done, clean up properly
            queue.clear();
        } catch (InvalidProtocolBufferException ex) {
            logger.error(ex);
        }
    }

    public int getHeartbeatInterval() {
        return heartbeatInterval;
    }

    public void setHeartbeatInterval(int heartbeatInterval) {
        this.heartbeatInterval = heartbeatInterval;
    }

    public String getFrontend() {
        return frontend;
    }

    public void setFrontend(String frontend) {
        this.frontend = frontend;
    }

    public String getBackend() {
        return backend;
    }

    public void setBackend(String backend) {
        this.backend = backend;
    }

    public int getHeartbeatLiveness() {
        return heartbeatLiveness;
    }

    public void setHeartbeatLiveness(int heartbeatLiveness) {
        this.heartbeatLiveness = heartbeatLiveness;
    }

    public Logger getLogger() {
        return logger;
    }

    public void setLogger(Logger logger) {
        this.logger = logger;
    }
}