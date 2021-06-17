const mytext = `import os 
import requests 
import pandas as pd 
subscription_key = "<subscription-key>" 
endpoint = "https://lunaai-api.azurewebsites.net" 
df = pd.read_csv("boston.csv")
url = endpoint + "/apiv2/testapp/predict/predict?api-version=v1"
response = requests.post(url, headers={"api-key": subscription_key}, json=df.to_dict('split')) 
if response.status_code == 200: 
    print(response.json())
`;

export default mytext ;