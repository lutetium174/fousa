 # Translator Agent

   Version: 1.0.0
   Description: A pi agent for translating text using Mistral as the engine.

   ## Functions

   ### translate

   #### Description

   Translate text using Mistral.

   #### Parameters

   | Name | Type   | Description               |
   |------|--------|---------------------------|
   | text | string | The text to translate     |
   | source | string | The source language       |
   | target | string | The target language       |
   | timeout | number | The timeout (optional)    |

   #### Returns

   The translated text.

   ## Integration with Web Applications

   The Translator Agent can be used by web applications to translate text. The workflow involves sending requests to the Translator Agent API and receiving the translated text as the
 response.

   1. User sends text to the web application's API.
   2. Web application parses and passes the request to the Translator Agent API.
   3. Translator Agent translates the text using Mistral and sends the response back to the web application.
   4. The web application returns the translated text to the user.

   ## Dependencies

   - Mistral CLI: <https://github.com/mistral-ai/mistral>

   ## Setup

   1. Install Mistral CLI on the system.
   2. Register the Translator Agent functions in the pi-coding-agent custom-providers.json file.
   3. Restart the pi-coding-agent service to load the new custom provider.