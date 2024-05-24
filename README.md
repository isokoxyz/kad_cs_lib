# Welcome to kad_cs_lib!
This library is intended to give C# developers the ability to execute transactions from their applications.

For example, Unity game developers could execute transactions from within their game using this library.

## Setup
### Prerequisites
1. Ensure machine or dev environment is capable of running C# projects (if you're using this in Unity or Visual Studio, this should be satisfied)
2. To run the examples.cs file in isolation, [set up your Visual Studio Code](https://code.visualstudio.com/docs/csharp/get-started) local dev environment to run C#

### Installation
#### For Windows: 
Install [Zelcore Wallet](https://zelcore.io/)
#### For MacOS: 
Install [Zelcore Wallet](https://zelcore.io/) \
OR \
Install [Chainweaver Wallet Desktop Application](https://docs.kadena.io/participate/wallets/chainweaver)

### Run Local Pact Server
* Run either Zelcore or Chainweaver, NOT both
* Log in to the wallet application
* If on Zelcore wallet, click on chain icon on the upper right corner
  * Set *PORT* to 9467
  * Ensure *Status* switch is toggled on

## Usage
### Running Example in Visual Studio Code
To run the **examples.cs** file for testing:
* Set the *adminAccount* value to your k-wallet address
* For exec commands:
  * Uncomment **execExample()** in the **Main()** function
  * Set the values of the *key* and *value* variables
* For cont commands:
  * Uncomment **contExample()** in the **Main()** function
  * Set the value of the *value2* variable

* Run the project file using the run and debug button on the top right of the dev window
* The transaction should be triggered and you will be prompted to sign the command in the wallet application you are using.\
\
***NOTE:*** Make sure the value of **key** is different with each run, as the date inserted in the tables by the contract must be uniquely indexed.

### Importing to existing codebase
Simply copy and paste the **kad_cs** directory into your codebase and use the classes and methods as normal