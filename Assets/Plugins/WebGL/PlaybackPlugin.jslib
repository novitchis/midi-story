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

  FileLoaded: function() {
    ReactUnityWebGL.FileLoaded();
  }
  
  ImageCaptured: function(name, pngBytes) {
    ReactUnityWebGL.ImageCaptured(name, pngBytes);
  }
});