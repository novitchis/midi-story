mergeInto(LibraryManager.library, {

  // Create a new function with the same name as
  // the event listeners name and make sure the
  // parameters match as well.

  Finished: function() {

    // Within the function we're going to trigger
    // the event within the ReactUnityWebGL object
    // which is exposed by the library to the window.

    ReactUnityWebGL.Finished();
  },

  FileLoaded: function(fileInfo) {
    ReactUnityWebGL.FileLoaded(JSON.parse(Pointer_stringify(fileInfo)));
  },
  
  ImageCaptured: function(name) {
    ReactUnityWebGL.ImageCaptured([Pointer_stringify(name), FS]);
  },
});