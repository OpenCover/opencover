
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.0.30319.1
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly extern OpenCover.Samples.Framework
{
  .publickeytoken = (3F 34 F3 51 0E 66 44 52 )
  .ver 1:0:0:0
}
.assembly OpenCover.Samples.IL
{
  .custom instance void [mscorlib]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ( 01 00 14 4F 70 65 6E 43 6F 76 65 72 2E 53 61 6D   // ...OpenCover.Sam
                                                                                              70 6C 65 73 2E 49 4C 00 00 )                      // ples.IL..
  .custom instance void [mscorlib]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 09 4D 69 63 72 6F 73 6F 66 74 00 00 )       // ...Microsoft..
  .custom instance void [mscorlib]System.Reflection.AssemblyProductAttribute::.ctor(string) = ( 01 00 14 4F 70 65 6E 43 6F 76 65 72 2E 53 61 6D   // ...OpenCover.Sam
                                                                                                70 6C 65 73 2E 43 53 00 00 )                      // ples.CS..
  .custom instance void [mscorlib]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ( 01 00 1B 43 6F 70 79 72 69 67 68 74 20 C2 A9 20   // ...Copyright .. 
                                                                                                  4D 69 63 72 6F 73 6F 66 74 20 32 30 31 31 00 00 ) // Microsoft 2011..
  .custom instance void [mscorlib]System.Reflection.AssemblyTrademarkAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.ComVisibleAttribute::.ctor(bool) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 37 35 34 37 30 37 38 37 2D 66 32 63 35   // ..$75470787-f2c5
                                                                                                  2D 34 66 30 35 2D 38 39 30 66 2D 63 34 31 62 34   // -4f05-890f-c41b4
                                                                                                  64 38 32 62 30 65 66 00 00 )                      // d82b0ef..
  .custom instance void [mscorlib]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ( 01 00 07 31 2E 30 2E 30 2E 30 00 00 )             // ...1.0.0.0..
  .custom instance void [mscorlib]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 1A 2E 4E 45 54 46 72 61 6D 65 77 6F 72 6B   // ....NETFramework
                                                                                                        2C 56 65 72 73 69 6F 6E 3D 76 34 2E 30 01 00 54   // ,Version=v4.0..T
                                                                                                        0E 14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C   // ..FrameworkDispl
                                                                                                        61 79 4E 61 6D 65 10 2E 4E 45 54 20 46 72 61 6D   // ayName..NET Fram
                                                                                                        65 77 6F 72 6B 20 34 )                            // ework 4

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 07 01 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}
.module OpenCover.Samples.IL.dll
// MVID: {91B3E716-4058-449E-8D68-0FA4723F1598}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x00320000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi beforefieldinit OpenCover.Samples.IL.TryFaultTarget
       extends [mscorlib]System.Object
{
  .field private initonly class [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery _query
  .method public hidebysig specialname rtspecialname 
          instance void  .ctor(class [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery query) cil managed
  {
    // Code size       17 (0x11)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  nop
    IL_0007:  nop
    IL_0008:  ldarg.0
    IL_0009:  ldarg.1
    IL_000a:  stfld      class [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery OpenCover.Samples.IL.TryFaultTarget::_query
    IL_000f:  nop
    IL_0010:  ret
  } // end of method TryFaultTarget::.ctor

  .method public hidebysig instance void 
          TryFault() cil managed
  {
    // Code size       34 (0x22)
    .maxstack  1
    IL_0000:  nop
    .try
    {
      IL_0001:  nop
      IL_0002:  ldarg.0
      IL_0003:  ldfld      class [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery OpenCover.Samples.IL.TryFaultTarget::_query
      IL_0008:  callvirt   instance bool [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery::ThrowException()
      IL_000d:  pop
      IL_000e:  nop
      IL_000f:  leave.s    IL_0020

    }  // end .try
    fault
    {
      IL_0011:  nop
      IL_0012:  ldarg.0
      IL_0013:  ldfld      class [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery OpenCover.Samples.IL.TryFaultTarget::_query
      IL_0018:  callvirt   instance void [OpenCover.Samples.Framework]OpenCover.Samples.Framework.ITestExceptionQuery::InFault()
      IL_001d:  nop
      IL_001e:  nop
      IL_001f:  endfinally
    }  // end handler
    IL_0020:  nop
    IL_0021:  ret
  } // end of method TryFaultTarget::TryFault

} // end of class OpenCover.Samples.IL.TryFaultTarget


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file OpenCover.Samples.IL.res
