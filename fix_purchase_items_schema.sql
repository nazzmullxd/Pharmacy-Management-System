-- Fix PurchaseItems table schema to allow null ProductBatchID
-- This resolves the foreign key constraint error when creating purchase orders

-- Step 1: Drop the existing foreign key constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PurchaseItems_ProductBatches_ProductBatchID')
BEGIN
    ALTER TABLE [PurchaseItems] DROP CONSTRAINT [FK_PurchaseItems_ProductBatches_ProductBatchID];
    PRINT 'Dropped existing foreign key constraint FK_PurchaseItems_ProductBatches_ProductBatchID';
END

-- Step 2: Make ProductBatchID column nullable
ALTER TABLE [PurchaseItems] 
ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;
PRINT 'Made ProductBatchID column nullable in PurchaseItems table';

-- Step 3: Recreate the foreign key constraint to allow NULL values
ALTER TABLE [PurchaseItems]
ADD CONSTRAINT [FK_PurchaseItems_ProductBatches_ProductBatchID] 
FOREIGN KEY ([ProductBatchID]) REFERENCES [ProductBatches]([ProductBatchID]);
PRINT 'Recreated foreign key constraint with NULL support';

-- Step 4: Verify the change
SELECT 
    c.COLUMN_NAME,
    c.IS_NULLABLE,
    c.DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME = 'PurchaseItems' 
AND c.COLUMN_NAME = 'ProductBatchID';

PRINT 'Schema update completed successfully!';