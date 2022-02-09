# HeuristicLabConnector

Connects the Heuristic Lab to awesome programming languages such as Java, via the [Amaru Framework](amaru.dev).


## Getting Started

The project is split into three parts.

A) HeuristicLab -> is the submodule in the csharp folder.
B) Broker - Contains the Java Broker that Heuristic Lab connects to
C) Client - Also available as sample in Java. A working implementation is included in [Amaru](amaru.dev).

The architecture is explained in the corresponding [publication](https://dl.acm.org/doi/10.1145/3377929.3398103)

To get started, first build the project:
> ./build_linux.sh
> cd ./java
> mvn clean install

To run first start the Broker:
> at.fh.hagenberg.aist.hlc.broker.Broker

Then the amaru client. An example in this project is:
> at.fh.hagenberg.aist.hlc.worker.ExampleExternalOptimizationWorker
The real worker in amaru for MiniC:
> at.fh.hagenberg.aist.gce.lang.optimization.minic.Main

Then HeuristicLab:
> mono "./csharp/HeuristicLab/bin/HeuristicLab 3.3.exe"

## Contributing

This work was created with the support of [Daniel Dorfmeister](https://github.com/ddorfmeister/) as part of my PHD thesis. Thank you!

If you want to contribute feel free to [contact me](https://github.com/oliver-krauss). You can also open issues directly in the repository.

## License

For HeuristicLab -> please see the [HeuristicLab repository](https://github.com/heal-research/HeuristicLab/). Heuristic Lab is published under the GPL v3

For Java:

Copyright (c) 2022 the original author or authors. DO NOT ALTER OR REMOVE COPYRIGHT NOTICES.

This Source Code Form is subject to the terms of the Mozilla Public License, v. 2.0. If a copy of the MPL was not
distributed with this file, You can obtain one at https://mozilla.org/MPL/2.0/.

## Research

If you are going to use this project as part of a research paper, we would ask you to reference this project by citing
it.

If you arrived here, you already got the Zenodo DOI.

