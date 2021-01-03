A simple solution to demonstrate how to use OpenCover with your project

1. restore solution packages using restore.bat
  - this downloads NUnit.Runners, ReportGenerator and OpenCover
2. compile test solution using Visual Studio
  - this should also download the Nunit package
3. perform coverage run using coverage.bat
  - this runs the NNnit tests using the NUnit.Runners and capturing the coverage using OpenCover
  - ReportGenerator is then used to turn this output into easy to read HTML