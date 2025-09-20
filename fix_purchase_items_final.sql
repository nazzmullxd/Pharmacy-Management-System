-- Fix PurchaseItems ProductBatchID to be nullable
-- This script dynamically finds the correct foreign key constraint name

DECLARE @ConstraintName NVARCHAR(255)

-- Find the actual foreign key constraint name
SELECT @ConstraintName = fk.name
FROM sys.foreign_keys fk
INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
WHERE tp.name = 'PurchaseItems' AND cp.name = 'ProductBatchID' AND tr.name = 'ProductBatches';

-- Drop the foreign key constraint if it exists
IF @ConstraintName IS NOT NULL
BEGIN
    DECLARE @DropSQL NVARCHAR(500)
    SET @DropSQL = 'ALTER TABLE [PurchaseItems] DROP CONSTRAINT [' + @ConstraintName + ']'
    EXEC sp_executesql @DropSQL
    PRINT 'Dropped foreign key constraint: ' + @ConstraintName
END
ELSE
BEGIN
    PRINT 'No foreign key constraint found for ProductBatchID in PurchaseItems table'
END

-- Make ProductBatchID column nullable
ALTER TABLE [PurchaseItems] 
ALTER COLUMN [ProductBatchID] uniqueidentifier NULL;
PRINT 'Made ProductBatchID column nullable in PurchaseItems table'

-- Recreate the foreign key constraint to allow NULL values
ALTER TABLE [PurchaseItems]
ADD CONSTRAINT [FK_PurchaseItems_ProductBatches_ProductBatchID] 
FOREIGN KEY ([ProductBatchID]) REFERENCES [ProductBatches]([ProductBatchID]);
PRINT 'Recreated foreign key constraint with NULL support'

-- Verify the change
SELECT 
    COLUMN_NAME,
    IS_NULLABLE,
    DATA_TYPE
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'PurchaseItems' AND COLUMN_NAME = 'ProductBatchID';

PRINT 'Schema update completed successfully!';