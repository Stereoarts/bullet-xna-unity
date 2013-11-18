bullet-xna-unity
================

bullet-xna
A C#/XNA port of the bullet physics library
porting for Unity.

Original Project Page.
https://code.google.com/p/bullet-xna/

Original Code Repository.
http://bullet-xna.googlecode.com/svn/trunk/ bullet-xna-read-only

How to use.

1. Using Visual C# Build( .dll )

Open BulletXNA-Unity.csproj in Visual C# and Build this.
Put BulletXNA-Unity.dll in your Unity project.

2. Using Source code in Unity.

Put all source code in your Unity project.
Define preprocessor "Unity".(dmcs.rsp, gmcs.rsp, and smcs.rsp.)

-define:UNITY

