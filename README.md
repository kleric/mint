# Mint
Tools for working with the Mabiongi mint scripts.

Currently fairly early/prototype stage. If you want to integrate
with Aura for example, you'll need to do a lot of integration work.

tl;dr
Scripts to convert go in scripts/mint folder
Generated scripts come out in scripts/csharp folder
Can add type information for methods in mint_interfaces. Name of
file should be type. See Prop.mint for examples.

If you want to change type mapping, see MintTypeMap.cs

## mint_interfaces folder
Folder for interfaces, saved as mint files, that present type information
for type inference. This is for fixing issues where generated files do things
such as:

    int x = _prop.GetPositionX();

when we actually want

    float x = _prop.GetPositionX();
