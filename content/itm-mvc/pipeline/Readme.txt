Configuration for bitbucket pipelines is contained in bitbucket-pipelines.yml

The following bitbucket pipeline environment variables must be set for it to work. Examples are written in parenthesis:


Azure deploy via FTP:
FTP_HOST (waws-prod-db3-xxx.ftp.azurewebsites.windows.net)
FTP_SITE_ROOT (/site/wwwroot)
FTP_USERNAME (testsite\testuser)
FTP_PASSWORD (password123)

Azure storage account for releases:
AZURE_STORAGE_ACCOUNT (storageaccount)
AZURE_STORAGE_CONTAINERNAME (containername)
AZURE_STORAGE_ACCESS_KEY (dALhRNx+6xz/KLuCk7mzpBHnY2/gn0kYBNNiqCfgPCJlZFq9FnEQJpj6oaYp102K9lSYJQd2JwYCL4tg/m13DA==)