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

package at.fh.hagenberg.aist.hlc.core;

import at.fh.hagenberg.aist.hlc.core.messages.*;
import com.google.protobuf.Message;

/**
 * Interfaces for workers. These workers must implement ALL features of this interface.
 * @author Oliver Krauss on 17.04.2019
 */
public interface ExternalOptimizationWorker {

    /**
     * Configures the worker and defines the problem
     * @param algorithmRunId ID of the algorithm run
     * @param request with configuration and problem information
     * @return outcome of the configuration and problem validations
     */
    StartAlgorithmResponse configure(String algorithmRunId, StartAlgorithmRequest request);

    /**
     * Executes an operator (create, evaluate, cross, mutate, ...)
     * @param algorithmRunId ID of the algorithm run
     * @param request with parameters of the operator
     * @return outcome of the operator
     */
    Message operate(String algorithmRunId, Message request);

    /**
     * Shuts down the truffle engine and clears memory related to this worker
     * @param algorithmRunId ID of the algorithm run
     * @param request with information how the algorithm was stopped
     */
    void shutdown(String algorithmRunId, StopAlgorithmRequest request);
}
