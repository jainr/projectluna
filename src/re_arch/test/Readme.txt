Automated Test Scripts for LunaAI

1. Locust.io Framework Test Scripts
Test scripts are developed utilizing https://locust.io/ framework.  
They can be run manually from the terminal in headless mode ( https://docs.locust.io/en/stable/running-locust-without-web-ui.html ) or automatically via ADO pipeline.

Each test method calls self._test() with the REST method name, whether to test for success/failure, the uri to call, header data, and body json data (optional depending on method)
    Example:
    self._test("put","success", uri, self.headerData, body)


2. Configuration JSON file
The luna_locust_config.json file contains all the environment configuration parameters that should need to be updated.


3. Azure DevOps Pipeline YAML Definitions
New Pipelines can be added in ADO using the FunctionalTests and ScnearioTests yaml files.  These yaml definitions will Install Locust and the run Locust in headless mode, passing in the test scripts.


4. GitHub Actions
Support for GitHub Actions is TBD...