VERSION = $(shell awk '/^v[0-9]/ {print $$1; exit }' CHANGELOG.md)
TARGET = AvatarDresser-$(VERSION).unitypackage

all: build

version:
	@echo $(VERSION)

Editor/Version.cs: CHANGELOG.md
	@sed -i 's/VERSION = ".*"/VERSION = "$(VERSION)"/' $@


$(TARGET): Editor/Version.cs
	# copy stuff to a tempdir to build our release tree
	mkdir -p .tmp/Assets/SophieBlue/AvatarDresser
	ls | grep -v "Assets" | xargs -i{} cp -a {} .tmp/Assets/SophieBlue/AvatarDresser/
	.github/workflows/generate_meta.sh bc846a2331c27846b961e0f9fe107d54 > .tmp/Assets/SophieBlue.meta
	.github/workflows/generate_meta.sh 35463f4647344b07b00095724986cf0a > .tmp/Assets/SophieBlue/AvatarDresser.meta

	# build the unity package
	cup -c 2 -o $@ -s .tmp
	mv .tmp/$@ .
	rm -rf .tmp

	# rebuild the unity package
	unzip -d .tmp $@
	rm $@
	cd .tmp && tar cvf ../$@ * && cd -
	rm -rf .tmp

build: $(TARGET)

clean:
	rm -f $(TARGET)
	rm -rf .tmp
.PHONY: clean
