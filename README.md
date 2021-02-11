# Publish and sell your ML model as a service in Azure using Project Luna
## Overview
Today, AI is playing a key role in so many areas businesses and our lifes. AI solutions evolves so fast that a new algorithm or technology can be out of date and replaced in months, sometimes weeks. For AI service providers, how to efficiently package, manage and publish machine learning models or algorithms into a sellable AI service become one of the key drivers for business success.

Project Luna allows you package your machine learning models and algorithms into subscribable AI services without writing additional code. Your data scientists can focus on improving your model while Project Luna already takes care of all other platform or service related work, including subscription management, authorization and authentication, telemetry and more.

Moreover, Project Luna also helps you publishing your AI services as an SaaS offer in Azure Marketplace and sell through Microsoft without extra coding effort. It will make your applications available to millions of Azure users and dramatically increase the visibility of your applications.

## What is Project Luna
Project Luna provides a solution template, which allows you publish your machine learning projects into AI services and publish the services in Azure Marketplace as an SaaS offer.
- Project Luna provides a project template which allows you define your own workflow
- Project Luna provides a management portal where you can publish your machine learning project into an API service directly from your Git (GitHub or Azure DevOps) repo
- Project Luna provides allows you register Azure Machine Learning workspace which provides auto-scalable compute clusters. When the user calling the APIs, your code will be running in those clusters without you worrying about scaling up and down compute resources.
- Project Luna provides a SaaS offer template for you to easily publish your AI service as an SaaS offer in Azure Marketplace. It allows you automate the resource provisioning by using ARM templates and webhooks
- Project Luna allows you configure usage based billing using your service telemetry data.

## Get Started
There're two key parts of Project Luna: 
- Luna.ai allows you manage and package your ML projects into AI services
- Luna for Azure Marketplace allows you publish AI services or existing applications in Azure Marketplace as SaaS offers

To start with Luna.ai, please see [the end to end Luna.ai tutorial](./Resources/Documentation/luna.ai/end-to-end-tutorial/README.md).

If you already have an application and just want to publish your application in Azure Marketplace, please see [Luna for Azure Marketplace](./Resources/Documentation/lunav1/README.md).
