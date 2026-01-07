# Bro-Maker Makefile
# Minimal MSBuild wrapper - leverages Scripts/BroforceModBuild.targets for installation

# Detect OS and set MSBuild path
ifeq ($(OS),Windows_NT)
    MSBUILD := /mnt/c/Program Files/Microsoft Visual Studio/2022/Community/MSBuild/Current/Bin/MSBuild.exe
else
    MSBUILD := msbuild
endif

MSBUILD_FLAGS := /p:Configuration=Release /verbosity:minimal /nologo

# LAUNCH variable controls both kill and launch behavior
# Usage: make LAUNCH=no
ifeq ($(LAUNCH),no)
	LAUNCH_FLAGS := /p:CloseBroforceOnBuild=false /p:LaunchBroforceOnBuild=false
else
	LAUNCH_FLAGS := /p:CloseBroforceOnBuild=true /p:LaunchBroforceOnBuild=true
endif

# Default target shows help
.DEFAULT_GOAL := help

.PHONY: help
help:
	@echo "Bro-Maker Build System"
	@echo ""
	@echo "Targets:"
	@echo "  make build              Build BroMakerLib (kill game, build, launch)"
	@echo "  make build-no-launch    Build without disrupting running game"
	@echo "  make clean              Clean build artifacts"
	@echo "  make rebuild            Clean and rebuild"
	@echo ""
	@echo "Options:"
	@echo "  LAUNCH=no               Don't kill or launch game"
	@echo ""
	@echo "Examples:"
	@echo "  make build              Standard build with game launch"
	@echo "  make build LAUNCH=no    Build without disrupting running game"

.PHONY: build
build:
	"$(MSBUILD)" BroMakerLib.sln $(MSBUILD_FLAGS) $(LAUNCH_FLAGS)

.PHONY: build-no-launch
build-no-launch:
	"$(MSBUILD)" BroMakerLib.sln $(MSBUILD_FLAGS) /p:CloseBroforceOnBuild=false /p:LaunchBroforceOnBuild=false

.PHONY: clean
clean:
	"$(MSBUILD)" BroMakerLib.sln /t:Clean $(MSBUILD_FLAGS)

.PHONY: rebuild
rebuild: clean
	"$(MSBUILD)" BroMakerLib.sln $(MSBUILD_FLAGS) /p:CloseBroforceOnBuild=false /p:LaunchBroforceOnBuild=false
