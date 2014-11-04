This folder contains shared source files that can be linked into multiple projects
as needed.  This can be valuable in (hopefully rare) scenarios where it is desirable
to share code, but infeasible or impractical to compile that shared code into a single
shared assembly.

Code in this folder is not considered to be "owned" by any one given project, but
rather, simply source controlled in a single place which is highly-visible in order
to emphasize that the code is not "owned" but rather, "shared".

The general approach is for an assembly to use the "Link File" method of including
the desired source code files into its project without moving or copying the files.

It should be remembered that since these source code files are potentially compiled
into multiple assemblies, the classes defined here should generally fall under the
"internal helper" category rather than the "public type" category because the runtime
type will actually be different for each assembly that compiles these source code
files ... thus making them generally unsuitable for use as shared public types.
