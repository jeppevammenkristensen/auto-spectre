# Releases

## 0.7.0

### AutoSpectre

* TextPrompt has three new properties for defining choices.
* DefaultValueSource added on TextPrompt

### AutoSpectre.SourceGeneration

* Based on the new properties in TextPrompt, the source generation has been updated to support these new properties.
* New solution for defining DefaultValueSource. Old way discarded.
* Fixed double diagnostics if source was missing
* Added support for defining Sources that are Static. And allow them to be fields
    1. Source on SelectPrompt now supports fields and static
    2. Converter on SelectPrompt now supports static method
    3. Validator on TextPrompt now supports static method.
    4. TypeInitializer on TextPrompt now supports static method.
    5. Choices on TextPrompt now supports static and has added fields

## 0.6.0

### Autospectre

* Added an extended Text prompt to allow for passing in a delegate to convert from string to the given type of the prompt
* Added extension method to allow setting the culture "fluently" used by source generation
* Added possibility to add custom extra text to TextPrompt title.

### AutoSpectre.SourceGeneration

* Fix to allow inheritance for form classes (classes decorated with AutoSpectreForm)
* Allow setting culture to use for source generation. The culture is initalized as a culture variable. Per default it will be the CurrentUICulture
* Added ability to point to a method to initialize anohter referenced form. This is useful if that class has a constructor that is not empty.


