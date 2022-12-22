VERSION = $(shell awk '/^v[0-9]/ {print $$1; exit }' CHANGELOG.md)

all: Editor/Version.cs

version:
	@echo $(VERSION)

Editor/Version.cs: CHANGELOG.md
	@sed -i 's/VERSION = ".*"/VERSION = "$(VERSION)"/' $@
