function marshalString(value) {
  // If value is null or undefined, treat it as an empty string
  if (value === undefined || value === null) {
    value = "";
  }

  // Always convert to string to be safe
  var str = String(value);

  // Allocate space for the UTF-8 bytes + null terminator
  var length = lengthBytesUTF8(str) + 1;
  var ptr = _malloc(length);
  if (!ptr) {
    return 0; // in case malloc failed
  }

  // Write into WASM memory
  stringToUTF8(str, ptr, length);
  return ptr;
}

async function initializeSdkAsync() {
  await new Promise((resolve, reject) => {
    const script = document.createElement('script');
    script.src = 'https://cdn.jest.com/sdk/latest/jestsdk.js';
    script.onload = resolve;
    script.onerror = reject;
    document.head.appendChild(script);
  });
  await window.JestSDK.init();
  console.log("Initialized.");
}

async function bootstrapSdkAsync(initializeSdkAsync) {
  try {
    // Do not start engine main() until we have downloaded & initialized the SDK
    Module.addRunDependency("initializeSdkAsync");
    await initializeSdkAsync();
  } catch (e) {
    console.error(`Unable to initialize SDK - ${e.message}`);
  } finally {
    Module.removeRunDependency("initializeSdkAsync");
  }
}

// Only run this code in the main thread
if (!Module.ENVIRONMENT_IS_PTHREAD) {
  Module.preRun.push(() => bootstrapSdkAsync(initializeSdkAsync));
}
