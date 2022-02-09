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

package at.fh.hagenberg.aist.hlc.worker;

import at.fh.hagenberg.aist.hlc.core.ConfigurationManagementWorker;
import at.fh.hagenberg.aist.hlc.core.ExternalOptimizationWorker;
import at.fh.hagenberg.aist.hlc.core.messages.*;
import org.springframework.context.support.ClassPathXmlApplicationContext;
import com.google.protobuf.Message;

import java.util.*;

/**
 * Dummy implementation for ExternalOptimizationWorker interface.
 *
 * @author Daniel Dorfmeister on 2019-07-17
 */
public class ExampleExternalOptimizationWorker implements ExternalOptimizationWorker, ConfigurationManagementWorker {

    private ExampleData data = new ExampleData();

    public static void main(String[] args) {
        ClassPathXmlApplicationContext configCtx = new ClassPathXmlApplicationContext("exampleConfig.xml");
        Worker worker = configCtx.getBean("worker", Worker.class);
        worker.work();
    }

    @Override
    public ConfigurationResponse getConfiguration(ConfigurationRequest request) {
        ConfigurationResponse response = ConfigurationResponse.newBuilder()
                .addAllLanguages(java.util.Collections.list(data.getLanguages().elements()))
                .setOptionGroup(data.getOptionGroup())
                .build();
        return response;
    }

    @Override
    public StartAlgorithmResponse configure(String algorithmRunId, StartAlgorithmRequest request) {
        String errorMessage = "";
        ProblemDefinition problemDefinition = request.getProblemDefinition();

        if (problemDefinition.getSourceCode().isEmpty()) {
            errorMessage = "No source code provided.";
        } else if (!problemDefinition.getSourceCode().contains(problemDefinition.getFunctionName())) {
            errorMessage = "Source code does not contain function '" + problemDefinition.getFunctionName() + "'.";
        } else { // check language and language-specific constraints
            if (problemDefinition.getLanguageId() == 0L) { // JavaScript
                if (problemDefinition.getInput().isEmpty()) {
                    errorMessage = "No input data provided.";
                } else if (problemDefinition.getOutput().split("\\n").length != problemDefinition.getInput().split("\\n").length) {
                    errorMessage = "Number of outputs must match number of inputs.";
                }
            } else if (problemDefinition.getLanguageId() == 1L) { // MiniC
                if (problemDefinition.getOutput().isEmpty()) {
                    errorMessage = "No output data provided.";
                }
            } else {
                errorMessage = "Unsupported language: try 'MiniC' or 'JavaScript'.";
            }
        }

        for (SymbolConfiguration config : request.getSymbolConfigurationList()) {
            long symbolId = config.getSymbolId();
            boolean enabled = config.getEnabled();
            double initialFrequency = config.getInitialFrequency();

            // symbol config is not used, as only predefined trees are available
        }

        for (Map.Entry<String, String> option : request.getOptionConfigurationMap().entrySet()) {
            String optionName = option.getKey(); // name of the option prefixed with higher-order options (separated by dot)
            String optionValue = option.getValue(); // value of the option, for ConstrainedOptions this is the index of the selected option

            // option config is not used, as only predefined trees are available
        }

        StartAlgorithmResponse response = StartAlgorithmResponse.newBuilder()
                .setSuccess(errorMessage.isEmpty())
                .setErrorMessage(errorMessage).build();
        return response;
    }

    @Override
    public void shutdown(String algorithmRunId, StopAlgorithmRequest request) {
        if (request.getExecutionState() == ExecutionState.STOPPED) {
            // free resources
        } else if (request.getExecutionState() == ExecutionState.PAUSED) {
            // do something
        }
    }

    @Override
    public Message operate(String algorithmRunId, Message request) {
        if (request.getClass().equals(SolutionCreatorRequest.class)) {
            return create(algorithmRunId, (SolutionCreatorRequest) request);
        } else if (request.getClass().equals(EvaluatorRequest.class)) {
            return evaluate(algorithmRunId, (EvaluatorRequest) request);
        } else if (request.getClass().equals(CrossoverRequest.class)) {
            return cross(algorithmRunId, (CrossoverRequest) request);
        } else if (request.getClass().equals(ManipulatorRequest.class)) {
            return mutate(algorithmRunId, (ManipulatorRequest) request);
        } else {
            throw new UnsupportedOperationException("Operator of type " + request.getClass() + " is not supported.");
        }
    }

    private SolutionCreatorResponse create(String algorithmRunId, SolutionCreatorRequest request) {
        Random rand = new Random();
        long solutionId =  rand.nextFloat() < 0.7 ? 1L : 2L;

        SolutionCreatorResponse response = SolutionCreatorResponse.newBuilder()
                .setSolutionId(solutionId)
                .setTree(data.getTrees().get(solutionId)).build();
        return response;
    }

    private EvaluatorResponse evaluate(String algorithmRunId, EvaluatorRequest request) {
        double quality = data.getQualities().get(request.getSolutionId());

        EvaluatorResponse response = EvaluatorResponse.newBuilder()
                .setSolutionId(request.getSolutionId())
                .setQuality(quality).build();
        return response;
    }

    private CrossoverResponse cross(String algorithmRunId, CrossoverRequest request) {
        long childTreeId = request.getParentSolutionId1(); // use first parent as child
        TreeNode childTree = data.getTrees().get(childTreeId);

        CrossoverResponse response = CrossoverResponse.newBuilder()
                .setParentSolutionId1(request.getParentSolutionId1())
                .setParentSolutionId2(request.getParentSolutionId2())
                .setChildSolutionId(childTreeId)
                .setTree(childTree).build();
        return response;
    }

    private ManipulatorResponse mutate(String algorithmRunId, ManipulatorRequest request) {
        long manipulatedTreeId = request.getSolutionId() == 1L ? 2L : 1L;
        TreeNode manipulatedTree = data.getTrees().get(manipulatedTreeId);

        ManipulatorResponse response = ManipulatorResponse.newBuilder()
                .setSolutionId(request.getSolutionId())
                .setManipulatedSolutionId(manipulatedTreeId)
                .setTree(manipulatedTree).build();
        return response;
    }
}
