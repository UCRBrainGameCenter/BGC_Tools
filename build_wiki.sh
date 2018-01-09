#!/bin/bash

doxygen doxygen.conf
moxygen out/xml
rm -r out
mv api.md BGC_Tools.wiki
cd BGC_Tools.wiki
git add api.md
git commit -m "new api commit"
git push
