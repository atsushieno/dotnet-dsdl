#!/bin/sh
cd Commons.Xml.Relaxng.Rnc && ../jay/jay -ctv <  ../jay/skeleton.cs RncParser.jay > RncParser.cs && cd ..
