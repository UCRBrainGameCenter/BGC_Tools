#!/bin/bash

if [ "$EUID" -ne 0 ]
  then echo "Please run as root"
  exit
fi

# clone required repos
cd ..
git clone https://github.com/UCRBrainGameCenter/BGC_Tools.wiki.git
git clone https://github.com/UCRBrainGameCenter/UCRBrainGameCenter.github.io.git
cd BGC_Tools

# generate documentation
doxygen doxygen.conf

# update html
mv out/html/* ../UCRBrainGameCenter.github.io
cd ../UCRBrainGameCenter.github.io
git add .
git commit -m "new api commit"
git push
cd ../BGC_Tools

# update wiki
moxygen out/xml
rm -r out
mv api.md ../BGC_Tools.wiki/Home.md
cd ../BGC_Tools.wiki
git add Home.md
git commit -m "new api commit"
git push
cd ..

# Delete wikis
rm -r BGC_Tools.wiki
rm -r UCRBrainGameCenter.github.io