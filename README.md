SSTC
Section Sag and Tension Calculator project related repositories.

This repo contains main application source files along with side libraries related with Section Sag and Tension Calculator application:

Related repositories:

I. SSTC_BaseModel - Model related classes and interfaces.

II. SSTC_MathModel - Clases responsible for calculating initial parameters, constants, Jacobian, etc. It contains also final results printer class. [REPOSITORY MARKED AS PRIVATE]

III. SSTC_MathOperationsLibrary - Repo that contains single static class with most basic math operations - used mostyly by MathModel objects.

IV. SSTC_Solver - An MVVM WPF solver module. Here are implemented all solving methods and algorithms which are responsible for... solving linear and nonlinear equations systems. Module has it's own view called "control" and set of exposed settings.

V. SSTC_Solver_CommonInterfaces - Set of interfaces tiding together MathModel and Solver modules.

VI. SSTC_ViewResources - A bunch of xaml views related resources like converters, regex based validators etc. nothing fancy :).

Additional Folders:

Lib - contains compiled libraries from aforementioned related repositories.

Misc - contains csv files with conductor/wire and insulator/attachnent sets default databases.

Database - contains binary databases which are used by SSTC application.

What is SSTC?

It's a MVVM WPF app (or at least something like that:). The program's main purpose is to calculate wire sags and tensions in overhead powerline section. Mathematically, the goal is achieved by solving 5th degree n - nonlinear equation system.

Main features:

Support for solving nonlinear equations systems by using the Newton-Raphson method with the possibility of selecting the Jacobi matrix factor algorithm.
User is able to define any overhead powerline section that includes: spacing, position in terrain and height of supporting structures, weight and length of wire attachment sets and additional loads from ice and wind.
Local databases of wires and insulator sets which provide a set of simple management tools.
Tab-based program design that enables the simultaneous modeling of several sections or several different load cases for a specific section.
Possibility of saving / reading to / from a file of both the entire project (section model, attached wire, preconditions, obtained results) or the model of the section itself.
Known issues:

Searching for a cable from the local database always works, however the results are not always shown. If this happens, click on the combo box that represents the selection of the type of a specific wire.
Databases are kept locally in binary files and loaded into memory when the program is started. There may be a situation where the file becomes corrupted for some unknown reason. Whole database can be then restored from the attached csv file in database management tab.
The program could yields a negative sag in specific conditions (that's not good :)). I am aware of that but so far I wasn't able to reproduce those conditions. I believe it is related someway with "h >> a" and vertex located very close to support point situation.
Development directions for the nearest future:

Module for support external database.
An estimator of additional ice and wind loads according to historical and current technical standards.
A setting tab, to change some in-app parameters.
The "drawer" module which will allow to present the obtained results in a graphic form.
The "printer" module which will allow the results to be printed to an xls/ods file.
An additional methods of solving nonlinear equations systems.
Credits: Piotr Gruszka SSTC Build: 082020 v0.85B
