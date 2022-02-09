FROM mono:6.4.0.198

# install Git
RUN apt-get update \
    && apt-get install -y git \
    && rm -rf /var/lib/apt/lists/*

# get latest commit so that cloning is executed again if repository changed
ADD https://api.github.com/repos/ddorfmeister/HeuristicLab/git/refs/ version.json
WORKDIR /app/build
# get source code
RUN git clone https://github.com/ddorfmeister/HeuristicLab.git .
# prepare project files and build HeuristicLab
RUN bash prepareProjectsForMono.sh \
    && nuget restore HeuristicLab.ExtLibs.sln \
    && nuget restore "HeuristicLab 3.3.sln" \
    && msbuild HeuristicLab.ExtLibs.sln \
    && msbuild "HeuristicLab 3.3.sln" \
    && cp -r /app/build/bin/ /app/bin/ \
    && rm -rf /app/build
