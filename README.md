# Project: Requesting multiple API
Request several companies' API for offers and select the best deal.

## Target Framework
.Net 6

## Project Structure
1. OfferAnalyzer - for requesting multiple API
2. OfferAnalyzerTest - for unit test

## Working

### Process Input:
- one set of data {{source address}, {destination address}, [{carton dimensions}]}
- Multiple API using the same data with different signatures

### Process Output:
- All API respond with the same data in different formats
- Process must query, then select the lowest offer and return it in the least amount of time
 
### Sample APIs, each with its own url and credentials

```bash
API1 (JSON)
                Input {contact address, warehouse address, package dimensions:[]}
                Output {total}
API2 (JSON)
                Input {consignee, consignor, cartons:[]}
                Output {amount}
API3 (XML)
                Input <xml><source/><destination/><packages><package/></packages></xml>
                Output <xml><quote/></xml>
```

## Implementation

1. Considered 3 sample APIs but can be increased as per requirement [Separate service classes will be required for handling their individual request/response].

2. Will return only one best deal, and return first offer if multiple deals found with same quote/amount.

3. One set of input data is passed to all apis.

4. API classes will be responsible for formatting request data.

5. Main class is OfferDataAnalyzer which is responsible to analyze multiple offers and return best deal.

6. API Processor is responsible for requesting multiple APIs.

7. API services will be called from their respective classes and will generate request/response in required format and apply required credentials and headers.

