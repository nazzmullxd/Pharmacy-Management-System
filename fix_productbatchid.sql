-- Make ProductBatchID nullable in PurchaseItems table
ALTER TABLE [PurchaseItems] 
ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;

-- Drop the foreign key constraint if it exists
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_PurchaseItems_ProductBatches')
BEGIN
    ALTER TABLE [PurchaseItems] DROP CONSTRAINT [FK_PurchaseItems_ProductBatches];
END

-- Recreate the foreign key constraint to allow NULL values
ALTER TABLE [PurchaseItems]
ADD CONSTRAINT [FK_PurchaseItems_ProductBatches] 
FOREIGN KEY ([ProductBatchID]) REFERENCES [ProductBatches]([ProductBatchID]);