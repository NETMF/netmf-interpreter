#### Re-thinking the NETMF Build infrastructure
When thinking about the future work for the NETMF build system there is one overarching high level
design goal, beyond the [functional requirements](BuildSystemRequirements.md):
Don't re-invent the wheel, leverage existing solutions to the maximum extent possible

The current build infrastructure is completely customized to NETMF with a long history of changes,
making it difficult to understand and maintain. Furthermore, the world around micro controllers
has changed significantly from the days where NETMF was first designed. At the time NETMF was
created there were no standards for low level SoC startup or driver APIs, every vendor had there
own way of doing things, if they provided any libraries at all. Thus, NETMF had to establish its
own HAL API to help in abstracting out the lower level. Fast forward to today and we have a
formally documented API model in CMSIS from ARM. While this is specific to ARM Cortex-M Micro
controllers the vast majority of it can apply to other CPUs. 

#### Re-usable software packs
Building applications from Software packages, whether it is Maven, Nuget, NPM, etc... the industry
as a whole has found significant value in the use of packaging code and build metadata into a
single downloadable versions package. In these systems the source code or binaries are bundled
into a container (usually ZIP compressed with some sort of index/metadata to describe the contents).
This allows an application to specify hard dependencies explicitly, including the exact version
without worry that newer version could break the application. CMSIS-PACK follows in this line
with specific support for the micro controller space. Of particular interest to the .NET Micro
Framework is the declaration of APIs and Requirements. 
