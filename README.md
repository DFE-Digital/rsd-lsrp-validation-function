# GitHub repository rsd-lsrp-validation-function

This repository contains a validation function for the RSD-LSRP project. The function is designed to validate input data and ensure that it meets the required specifications before processing.
FlexForms is configured to send a message topic to Service Bus when a file is uploaded to the FlexForms portal. The validation function is triggered by this message and performs the necessary validations.

## FileValidator

The FileValidator is responsible for validating the structure and content of input spreadsheets for the RSD-LSRP project. 
It checks for the presence of required worksheets, validates specific cell values, and ensures that data columns meet the expected criteria.
The function is triggered by a service bus message, which contains details of the spreadsheet to be validated. 
Performs the necessary validations, and returns a validation result indicating whether the input data is valid or if there are any errors that need to be addressed.
Only basic validation is performed on the input data, and it is expected that the data has already been pre-processed and cleaned before being passed to the FileValidator.

### Validation error messages ###

Note that the Azure Web Application Firewall (WAF) may block certain error messages from being posted to the FlexForms validation result API.
Rather than turning off those firewall rules, tailor error messages to avoid triggering the WAF, if possible.

For example, the following error texts are blocked by WAF (403), as it probably interprets it as a potential SQL injection attack:
- 'lsrp-quarterly-return-'
- 2025-26 or 2026-27

But the following error texts are allowed by WAF (200):
- lsrp-quarterly-return-
- 2026-27

WAF security errors return a 403 (forbidden) response and appear in a HTML document in the response body. WAF security error response extract:
```
<h1 class="govuk-heading-xl">Sorry, there is a problem with the service</h1>
<p class="govuk-body-l">Our security system detected an issue with the information you submitted.</p>
```

### Azure deployment ###

The FileValidator function is deployed to an Azure Function App. The deployment process involves setting up the necessary Azure resources, configuring the function app settings, and deploying the code to the Azure environment.
The function Service Bus trigger uses Topic and Subscription defined locally in the local.settings.json file, and remotely in the Azure Function App settings. 
The Service Bus connection string (Service Bus) is defined locally in user secrets and remotely in the Azure Function App connection strings. The connection string is not stored in the repository for security reasons.

The Common Architecture Team (CAT) is responsible for spinning up the Azure Function App and Service Bus resources, and for providing the connection string to the RSD-LSRP team. 
The RSD-LSRP team is responsible for deploying the FileValidator function to the Azure Function App and configuring the necessary settings.

Azure function app resources:
- development: s184d01-rsd-lsrp-val
- test: s184t01-rsd-lsrp-val
- production: s184p01-rsd-lsrp-val

Azure service bus resources:
- development: s184d01-rsdshared
- test: s184t01-rsdshared
- production: s184p01-rsdshared

The service bus connection string can be found in the Azure portal under the Service Bus namespace, in the "Shared access policies" section. The connection string should be copied and stored securely, as it is required for the FileValidator function to connect to the Service Bus.

FlexForms validation result API endpoint (ValidationResultFilesUrl) and API key (ValidationResultApiKey) is defined in the local.settings.json file, and remotely in the Azure Function App settings. The endpoint is used by the FileValidator function to post validation results back to FlexForms.
For information on how to create an API key, please refer to the FlexForms documentation or contact the FlexForms support team.
