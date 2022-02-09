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

import at.fh.hagenberg.aist.hlc.core.messages.*;

import java.time.Duration;
import java.time.LocalDateTime;
import java.util.Arrays;
import java.util.Dictionary;
import java.util.Hashtable;

public class ExampleData {
    private static Symbol symbolBase = Symbol.newBuilder()
            .setMinimumArity(2)
            .setMaximumArity(2)
            .setInitialFrequency(1.0F)
            .setEnabled(true)
            .build();

    private static Dictionary<String, Symbol> symbols = new Hashtable<String, Symbol>() {
        {
            put("+", symbolBase.toBuilder()
                    .setId(111L)
                    .setName("Addition")
                    .setDescription("Symbol that represents the + operator.")
                    .addAllowedChildSymbols(ChildSymbolConfiguration.newBuilder().setChildId(100L))
                    .build());
            put("-", symbolBase.toBuilder()
                    .setId(112L)
                    .setName("Subtraction")
                    .setDescription("Symbol that represents the - operator.")
                    .addAllowedChildSymbols(ChildSymbolConfiguration.newBuilder().setChildId(100L))
                    .build());
            put("-c", symbolBase.toBuilder()
                    .setId(113L)
                    .setName("Subtraction")
                    .setDescription("Symbol that represents the - operator. Second operand must be a constant.")
                    .addAllAllowedChildSymbols(Arrays.asList(
                            ChildSymbolConfiguration.newBuilder().setChildId(100L).addArgumentIndices(0).build(),
                            ChildSymbolConfiguration.newBuilder().setChildId(121L).addArgumentIndices(1).build()
                    ))
                    .build());
            put("c", symbolBase.toBuilder()
                    .setId(121L)
                    .setName("Constant")
                    .setDescription("Represents a constant value.")
                    .setMinimumArity(0)
                    .setMaximumArity(0)
                    .build());
            put("x", symbolBase.toBuilder()
                    .setId(122L)
                    .setName("Variable")
                    .setDescription("Represents a variable value.")
                    .setMinimumArity(0)
                    .setMaximumArity(0)
                    .build());
            put("===", symbolBase.toBuilder()
                    .setId(201L)
                    .setName("Equal Value And Equal Type")
                    .setDescription("Symbol that represents the === operator.")
                    .setEnabled(false)
                    // just an example for allowed child symbols, does not really make sense
                    .addAllowedChildSymbols(ChildSymbolConfiguration.newBuilder()
                            .setChildId(121L)
                            .addArgumentIndices(1))
                    .addAllowedChildSymbols(ChildSymbolConfiguration.newBuilder()
                            .setChildId(122L)
                            .addAllArgumentIndices(Arrays.asList(0, 1)))
                    .build());
        }
    };

    private static Language languageBase = Language.newBuilder()
            .addGroups(GroupSymbol.newBuilder()
                    .setId(100L)
                    .setName("Real Valued Symbols")
                    .addGroups(GroupSymbol.newBuilder()
                            .setId(110L)
                            .setName("Arithmetic Operators")
                            .addSymbols(symbols.get("+"))
                            .addSymbols(symbols.get("-"))
                            .addSymbols(symbols.get("-c")))
                    .addGroups(GroupSymbol.newBuilder()
                            .setId(120L)
                            .setName("Terminals")
                            .addSymbols(symbols.get("c"))
                            .addSymbols(symbols.get("x"))))
            .build();

    private static Dictionary<Long, Language> languages = new Hashtable<Long, Language>() {
        {
            put(0L, languageBase.toBuilder().setId(0L).setName("JavaScript")
                    .setDescription("JavaScript Grammar")
                    .addGroups(GroupSymbol.newBuilder()
                            .setId(200L)
                            .setName("Comparison Operators")
                            .addSymbols(symbols.get("===")))
                    .build());
            put(1L, languageBase.toBuilder().setId(1L).setName("MiniC").setDescription("MiniC Grammar").build());
        }
    };

    private static Dictionary<Long, TreeNode> trees = new Hashtable<Long, TreeNode>() {
        {
            put(1L, TreeNode.newBuilder()
                    .setId(101L).setSymbolId(symbols.get("+").getId())
                    .addChildren(TreeNode.newBuilder()
                            .setId(102L).setSymbolId(symbols.get("-").getId())
                            .addChildren(TreeNode.newBuilder()
                                    .setId(103L).setSymbolId(symbols.get("c").getId()))
                            .addChildren(TreeNode.newBuilder()
                                    .setId(104L).setSymbolId(symbols.get("c").getId())))
                    .addChildren(TreeNode.newBuilder()
                            .setId(105L).setSymbolId(symbols.get("x").getId()))
                    .build());
            put(2L, TreeNode.newBuilder() // simplified tree assuming c == 0.6
                    .setId(201L).setSymbolId(symbols.get("+").getId())
                    .addChildren(TreeNode.newBuilder()
                            .setId(202L).setSymbolId(symbols.get("c").getId()))
                    .addChildren(TreeNode.newBuilder()
                            .setId(203L).setSymbolId(symbols.get("x").getId()))
                    .build());
        }
    };

    private static Dictionary<Long, Double> qualities = new Hashtable<Long, Double>() {
        {
            put(1L, 5.0);
            put(2L, 3.0);
        }
    };

    private static OptionGroup optionGroup = OptionGroup.newBuilder()
            .addConstrainedOptions(ConstrainedOption.newBuilder()
                    .setName("SolutionCreator")
                    .setDescription("Solution creator.")
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("DefaultCreator")
                            .setDescription("Default creator.")
                            .addMultiOptions(MultiOption.newBuilder()
                                    .setName("Chooser")
                                    .setDescription("Chooser chooses.")
                                    .addItems(OptionGroup.newBuilder()
                                            .setName("RandomChooser")
                                            .setDescription("RandomChooser chooses randomly.")
                                            .addConstrainedOptions(ConstrainedOption.newBuilder()
                                                    .setName("Random")
                                                    .setDescription("Randomizer randomizes randomly.")
                                                    .addValidValues(OptionGroup.newBuilder()
                                                            .setName("Random")
                                                            .setDescription("Basic randomizer.")
                                                            .addOptions(Option.newBuilder()
                                                                    .setName("SetSeedRandomly")
                                                                    .setDescription("True if the random seed should be set to a random value, otherwise false.")
                                                                    .setType(OptionType.BOOL)
                                                                    .setDefault(Boolean.toString(true))
                                                            )
                                                    )
                                            )
                                    )
                                    .addItems(OptionGroup.newBuilder()
                                            .setName("BiasedChooser")
                                    )
                                    .addItems(OptionGroup.newBuilder()
                                            .setName("PatternMiningChooser")
                                    )
                                    .addAllDefaults(Arrays.asList(2))
                            )
                            .addOptions(Option.newBuilder()
                                    .setName("Seed")
                                    .setDescription("Seed")
                                    .setType(OptionType.INT)
                                    .setDefault(Integer.toString(89132781))
                            )
                    )
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("SimpleCreator")
                            .setDescription("Simple creator.")
                            .addOptions(Option.newBuilder()
                                    .setName("MyParam")
                                    .setDescription("MyDescription")
                                    .setType(OptionType.BOOL)
                                    .setDefault(Boolean.toString(true))
                            )
                    )
                    .setDefault(1)
            )
            .addConstrainedOptions(ConstrainedOption.newBuilder()
                    .setName("Crossover")
                    .setDescription("Crossover.")
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("DefaultCrossover")
                            .setDescription("Default crossover.")
                    )
            )
            .addConstrainedOptions(ConstrainedOption.newBuilder()
                    .setName("Mutator")
                    .setDescription("Mutator.")
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("DefaultMutator")
                            .setDescription("Default mutator.")
                    )
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("AnotherMutator")
                            .setDescription("Another mutator.")
                            .addOptions(Option.newBuilder()
                                    .setName("DoubleParam")
                                    .setType(OptionType.DOUBLE)
                                    .setDefault(Double.toString(0.123))
                            )
                            .addOptions(Option.newBuilder()
                                    .setName("PercentParam")
                                    .setType(OptionType.PERCENT)
                                    .setDefault(Double.toString(0.456))
                            )
                            .addOptions(Option.newBuilder()
                                    .setName("TimeSpanParam")
                                    .setType(OptionType.TIME_SPAN)
                                    .setDefault(Duration.ofSeconds(83).toString())
                            )
                            .addOptions(Option.newBuilder()
                                    .setName("DateTimeParam")
                                    .setType(OptionType.DATE_TIME)
                                    .setDefault(LocalDateTime.now().toString())
                            )
                    )
            )
            .addConstrainedOptions(ConstrainedOption.newBuilder()
                    .setName("Evaluator")
                    .setDescription("Evaluator.")
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("DefaultEvaluator")
                            .setDescription("Default evaluator.")
                            .addConstrainedOptions(ConstrainedOption.newBuilder()
                                    .setName("FitnessFunction")
                                    .setDescription("Represents a fitness function.")
                                    .addValidValues(OptionGroup.newBuilder()
                                            .setName("Fitness Function 1")
                                            .setDescription("Best fitness function of all time!")
                                    )
                                    .addValidValues(OptionGroup.newBuilder()
                                            .setName("Fitness Function 2")
                                            .setDescription("Now even better!")
                                    )
                            )
                    )
            )
            .addConstrainedOptions(ConstrainedOption.newBuilder()
                    .setName("AdditionalSettings")
                    .setDescription("Additional settings.")
                    .addValidValues(OptionGroup.newBuilder()
                            .setName("AdditionalSettings")
                            .setDescription("Default evaluator.")
                            .addOptions(Option.newBuilder()
                                    .setName("SomeSetting")
                                    .setDescription("Some setting")
                                    .setType(OptionType.INT)
                                    .setDefault(Integer.toString(42))
                            )
                    )
            )
            .addOptions(Option.newBuilder()
                    .setName("SomeImportantSetting")
                    .setDescription("Some important setting")
                    .setType(OptionType.BOOL)
                    .setDefault(Boolean.toString(true))
            )
            .build();

    public Dictionary<String, Symbol> getSymbols() {
        return symbols;
    }

    public Language getLanguageBase() {
        return languageBase;
    }

    public Dictionary<Long, Language> getLanguages() {
        return languages;
    }

    public Dictionary<Long, TreeNode> getTrees() {
        return trees;
    }

    public Dictionary<Long, Double> getQualities() {
        return qualities;
    }

    public OptionGroup getOptionGroup() {
        return optionGroup;
    }
}
