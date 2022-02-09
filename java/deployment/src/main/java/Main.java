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

import at.fh.hagenberg.aist.hlc.worker.ExampleExternalOptimizationWorker;
import com.google.protobuf.InvalidProtocolBufferException;

/**
 * Contains the main method that connects to HeuristicLab.TruffleConnector.
 *
 * @author Daniel Dorfmeister on 2019-06-05
 */
public class Main {

    /**
     * Receives messages from HeuristicLab.TruffleConnector and sends responses.
     */
    public static void main(String[] args) throws InvalidProtocolBufferException {
        ExampleExternalOptimizationWorker.main(args);
    }
}
