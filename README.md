# Learning Antlr4

The objective of this project is to explore the feature of Antlr4.

## Stack

🐍 **Python** to install antlr4-tools and **dotnet 6.0**

## Installing

Use this python package to help install the antlr4 tools to compile the grammar:

``` bash
  pip install antlr4-tools
```

Download the grammars you want to parse from https://github.com/antlr/grammars-v4 .
Then use this command to generate the code from grammar:

``` bash
  antlr4 -Dlanguage=CSharp MyGrammar.g4
```

Then to build / run the dotnet part of the project we need
``` bash
    dotnet restore
    dotnet build
    dotnet run
```

## SQL Parser

In this project, we will explore how to parse a T-SQL script and get all tables from the scripts. It's a simple example, but open possibilities to validate some SQL script parts or generate some code.

``` bash
    antlr4 -Dlanguage=CSharp grammars/tsql/TSqlLexer.g4 grammars/tsql/TSqlParser.g4 -o Learn.Antlr4/Antlr4.Grammars.TSql/ -package Antlr4.Grammars.TSql
```

Now that the code is generate lets create simple listener with a test.

## CSharp Parser

In this part of the project, we will explore hot to parse a CSharp file from a "legacy" code and try to export a model to use on a code generator.

``` bash
    antlr4 -Dlanguage=CSharp grammars/csharp/CSharpLexer.g4 grammars/csharp/CSharpParser.g4 grammars/csharp/CSharpPreprocessorParser.g4 -o Learn.Antlr4/Antlr4.Grammars.CSharp/ -package Antlr4.Grammars.CSharp
```