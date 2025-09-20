-- Simple approach: Just make the column nullable
-- The foreign key constraint will automatically handle NULL values

-- Step 1: Make ProductBatchID column nullable
ALTER TABLE [PurchaseItems] 
ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;

-- Step 2: Verify the change
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PurchaseItems' AND COLUMN_NAME = 'ProductBatchID';

PRINT 'ProductBatchID is now nullable. Purchase orders can be created without ProductBatchID!';