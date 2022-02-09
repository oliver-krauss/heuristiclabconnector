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

import at.fh.hagenberg.aist.hlc.core.messages.ConfigurationRequest;
import at.fh.hagenberg.aist.hlc.core.messages.ConfigurationResponse;

/**
 * Interfaces for configuration management workers.
 * @author Daniel Dorfmeister on 2019-09-11
 */
public interface ConfigurationManagementWorker {

    /**
     * Gets the languages and operators supported by the workers.
     * @param request (currently empty).
     * @return all supported languages and operators and their configuration options
     */
    ConfigurationResponse getConfiguration(ConfigurationRequest request);
}
