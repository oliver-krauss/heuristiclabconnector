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

import at.fh.hagenberg.aist.hlc.core.UuidHelper;
import org.zeromq.ZFrame;

import java.util.List;
/**
 * Represents a Paranoid Pirate worker, based on http://zguide.zeromq.org/java:ppqueue.
 *
 * @author Daniel Dorfmeister on 2019-06-19
 */
public class Worker {
    private ZFrame address;  //  Address of worker
    private String identity; //  Printable identity
    private long expiry;   //  Expires at this time

    private int heartbeatInterval;
    private int heartbeatLiveness;

    private List<Long> supportedLanguages;
    private boolean isConfigWorker;

    protected Worker(ZFrame address, List<Long> supportedLanguages, boolean isConfigWorker, int heartbeatInterval, int heartbeatLiveness) {
        this.address = address;
        this.supportedLanguages = supportedLanguages;
        this.isConfigWorker = isConfigWorker;
        this.heartbeatInterval = heartbeatInterval;
        this.heartbeatLiveness = heartbeatLiveness;

        this.identity = UuidHelper.getUUIDFromBytes(address.getData()).toString();
        resetExpiry();
    }

    /**
     * Resets the expiry time of the worker.
     */
    public void resetExpiry() {
        expiry = System.currentTimeMillis() + heartbeatInterval * heartbeatLiveness;
    }

    public ZFrame getAddress() {
        // if frame is sent, its memory is freed, which also frees the workers address
        // thus, return a copy of the address
        return address.duplicate();
    }

    public String getIdentity() {
        return identity;
    }

    public long getExpiry() {
        return expiry;
    }

    public List<Long> getSupportedLanguages() {
        return supportedLanguages;
    }

    public boolean isConfigWorker() {
        return isConfigWorker;
    }

    @Override
    public String toString() {
        StringBuilder sb = new StringBuilder("[");
        sb.append(identity);
        sb.append(", config: ");
        sb.append(isConfigWorker);
        sb.append(", languages: ");
        sb.append(supportedLanguages);
        sb.append("]");
        return sb.toString();
    }
}
