// Licensed to the Biwen.Settings under one or more agreements.
// The Biwen.Settings licenses this file to you under the MIT license. 
// See the LICENSE file in the project root for more information.
// Biwen.Settings Author: 万雅虎, Github: https://github.com/vipwan
// Biwen.Settings ,NET8+ 应用配置项管理模块
// Modify Date: 2024-09-18 17:31:11 _GlobalUsings.cs

global using FluentValidation;
global using FluentValidation.Results;
global using Microsoft.Extensions.AsyncState;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Options;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Localization;
global using System;
global using System.Collections.Concurrent;
global using System.Collections.Generic;
global using System.ComponentModel;
global using System.Linq;
global using System.Reflection;
global using System.Text.Json.Nodes;
global using Biwen.Settings;
global using Biwen.Settings.Domains;
global using FindTypes = Biwen.Settings.Infrastructure.TypeFinder.FindTypes;
global using ASS = Biwen.Settings.Infrastructure.Assemblies;
global using MSDA = System.ComponentModel.DataAnnotations;