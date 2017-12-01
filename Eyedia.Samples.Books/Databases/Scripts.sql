CREATE DATABASE Books   
    ON (FILENAME = 'C:\Workspace\GitHub\Samples\Eyedia.Samples.Books\Databases\Books.mdf'),   
    (FILENAME = 'C:\Workspace\GitHub\Samples\Eyedia.Samples.Books\Databases\Books_Log.ldf')   
    FOR ATTACH; 


EXEC sp_detach_db 'Books', 'true';