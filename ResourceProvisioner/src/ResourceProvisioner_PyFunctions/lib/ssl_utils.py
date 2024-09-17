import os
import requests
import logging

def configure_ca_root_certs():
    """
    Configures the CA root certificates for the requests module.

    Returns:
        None

    """
    # Set the CA root certificates for the requests module
    # check if the SSL_CERT_FILE environment variable is sets
    if 'SSL_CERT_FILE' in os.environ:   
        cert_file = os.environ['SSL_CERT_FILE']
        os.environ['REQUESTS_CA_BUNDLE'] = cert_file
        print(f'SSL_CERT_FILE environment variable is set to {cert_file}. Setting the CA root certificates for the requests module.')
        logging.info(f'SSL_CERT_FILE environment variable is set to {cert_file}. Setting the CA root certificates for the requests module.')    
        requests.utils.DEFAULT_CA_BUNDLE_PATH = cert_file
        requests.adapters.DEFAULT_CA_BUNDLE_PATH = cert_file
    #os.path.join(os.environ['AzureWebJobsScriptRoot'], 'lib', 'cacert.pem')
