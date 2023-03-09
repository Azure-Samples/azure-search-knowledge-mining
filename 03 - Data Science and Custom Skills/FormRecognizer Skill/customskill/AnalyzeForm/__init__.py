import logging
import json
import os
import logging
import pathlib
from azure.core.exceptions import ResourceNotFoundError
from azure.ai.formrecognizer import FormRecognizerClient
from azure.ai.formrecognizer import FormTrainingClient
from azure.core.credentials import AzureKeyCredential
import azure.functions as func

def main(req: func.HttpRequest) -> func.HttpResponse:
    logging.info('Invoked AnalyzeForm Skill.')
    try:
        body = json.dumps(req.get_json())
        if body:
            # For testing uncomment the following line to log the incoming request
            #logging.info(body)
            result = compose_response(body)
            return func.HttpResponse(result, mimetype="application/json")
        else:
            return func.HttpResponse(
                "The body of the request could not be parsed",
                status_code=400
            )
    except ValueError:
        return func.HttpResponse(
             "The body of the request could not be parsed",
             status_code=400
        )
    except KeyError:
        return func.HttpResponse(   
             "Skill configuration error. Endpoint, key and model_id required.",
             status_code=400
        )
    except AssertionError  as error:
        return func.HttpResponse(   
             "Request format is not a valid custom skill input",
             status_code=400
        )

def compose_response(json_data):
    body  = json.loads(json_data)
    assert ('values' in body), "request does not implement the custom skill interface"
    values = body['values']
    # Prepare the Output before the loop
    results = {}
    results["values"] = []
    mappings = None
    with open(pathlib.Path(__file__).parent / 'field_mappings.json') as file:
        mappings = json.loads(file.read())
    endpoint = os.environ["FORMS_RECOGNIZER_ENDPOINT"]
    key = os.environ["FORMS_RECOGNIZER_KEY"]
    model_id = os.environ["FORMS_RECOGNIZER_MODEL_ID"]
    form_recognizer_client = FormRecognizerClient(endpoint, AzureKeyCredential(key))
    for value in values:
        output_record = transform_value(value, mappings, form_recognizer_client, model_id)
        if output_record != None:
            results["values"].append(output_record)
            break
    return json.dumps(results, ensure_ascii=False)

## Perform an operation on a record
def transform_value(value, mappings, form_recognizer_client,model_id):
    try:
        recordId = value['recordId']
    except AssertionError  as error:
        return None
    try:         
        assert ('data' in value), "'data' field is required."
        data = value['data']        
        formUrl = data['formUrl']
        formSasToken = data ['formSasToken']
        formUrl = formUrl + formSasToken
        poller = form_recognizer_client.begin_recognize_custom_forms_from_url(
        model_id=model_id, form_url=formUrl)
        result = poller.result()
        recognized = {}
        for recognized_form in result:
            print("Form type: {}".format(recognized_form.form_type))
            for name, field in recognized_form.fields.items():
                label = field.label_data.text if field.label_data else name
                for (k, v) in mappings.items(): 
                    if(label == k):
                        recognized[v] =  field.value 

    except AssertionError  as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + error.args[0] }   ]       
            })
    except Exception as error:
        return (
            {
            "recordId": recordId,
            "errors": [ { "message": "Error:" + str(error) }   ]       
            })
    return ({
            "recordId": recordId,   
            "data": {
                "recognized": recognized
            }
            })  
