IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'My Store')
BEGIN
    CREATE DATABASE [My Store];
END
GO

USE [My Store];
GO

IF OBJECT_ID(N'[dbo].[User]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[User] (
        [Username]  NVARCHAR (255) NOT NULL,
        [Password]  NVARCHAR (255) NOT NULL,
        [UserRole]  NCHAR (1)      NOT NULL,
        PRIMARY KEY CLUSTERED ([Username] ASC)
    );
END
GO

IF OBJECT_ID(N'[dbo].[Customer]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Customer] (
        [CustomerID]      INT            IDENTITY (1, 1) NOT NULL,
        [CustomerName]    NVARCHAR (MAX) NOT NULL,
        [CustomerPhone]   NVARCHAR (15)  NOT NULL,
        [CustomerEmail]   NVARCHAR (MAX) NULL,
        [CustomerAddress] NVARCHAR (MAX) NULL,
        [Username]        NVARCHAR (255) NOT NULL,
        PRIMARY KEY CLUSTERED ([CustomerID] ASC),
        CONSTRAINT [FK_User_Customer] FOREIGN KEY ([Username]) REFERENCES [dbo].[User] ([Username])
    );
END
GO

IF OBJECT_ID(N'[dbo].[Category]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Category] (
        [CategoryID]   INT            IDENTITY (1, 1) NOT NULL,
        [CategoryName] NVARCHAR (MAX) NOT NULL,
        PRIMARY KEY CLUSTERED ([CategoryID] ASC)
    );
END
GO

IF OBJECT_ID(N'[dbo].[Product]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Product] (
        [ProductID]         INT             IDENTITY (1, 1) NOT NULL,
        [CategoryID]        INT             NOT NULL,
        [ProductName]       NVARCHAR (MAX)  NOT NULL,
        [ProductDecription] NVARCHAR (MAX)  NOT NULL,
        [ProductPrice]      DECIMAL (18, 2) NOT NULL,
        [ProductImage]      NVARCHAR (MAX)  NULL,
        PRIMARY KEY CLUSTERED ([ProductID] ASC),
        CONSTRAINT [FK_Pro_Category] FOREIGN KEY ([CategoryID]) REFERENCES [dbo].[Category] ([CategoryID])
    );
END
GO

IF OBJECT_ID(N'[dbo].[Order]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Order] (
        [OrderID]         INT             IDENTITY (1, 1) NOT NULL,
        [CustomerID]      INT             NOT NULL,
        [OrderDate]       DATE            NOT NULL,
        [TotalAmount]     DECIMAL (18, 2) NOT NULL,
        [PaymentStatus]   NVARCHAR (MAX)  NULL,
        [AddressDelivery] NVARCHAR (MAX)  NOT NULL,
        PRIMARY KEY CLUSTERED ([OrderID] ASC),
        FOREIGN KEY ([CustomerID]) REFERENCES [dbo].[Customer] ([CustomerID])
    );
END
GO

IF OBJECT_ID(N'[dbo].[OrderDetail]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderDetail] (
        [ID]        INT             IDENTITY (1, 1) NOT NULL,
        [ProductID] INT             NOT NULL,
        [OrderID]   INT             NOT NULL,
        [Quantity]  INT             NOT NULL,
        [UnitPrice] DECIMAL (18, 2) NOT NULL,
        PRIMARY KEY CLUSTERED ([ID] ASC),
        FOREIGN KEY ([ProductID]) REFERENCES [dbo].[Product] ([ProductID]),
        FOREIGN KEY ([OrderID]) REFERENCES [dbo].[Order] ([OrderID])
    );
END
GO

IF COL_LENGTH(N'dbo.OrderDetail', N'TotalPrice') IS NULL
BEGIN
    ALTER TABLE OrderDetail 
    ADD TotalPrice AS (Quantity * UnitPrice) PERSISTED;
END
GO

IF COL_LENGTH(N'dbo.Product', N'ProductDecription') IS NOT NULL
BEGIN
    EXEC sp_rename 'Product.ProductDecription', 'ProductDescription', 'COLUMN';
END
GO

IF COL_LENGTH(N'dbo.Order', N'AddressDelivery') IS NOT NULL
BEGIN
    EXEC sp_rename '[Order].AddressDelivery', 'ShippingAddress', 'COLUMN';
END
GO

IF COL_LENGTH(N'dbo.Order', N'DeliveryMethod') IS NULL
BEGIN
    ALTER TABLE [Order]
    ADD DeliveryMethod NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'dbo.Order', N'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE [Order]
    ADD PaymentMethod NVARCHAR(MAX) NULL;
END
GO

IF COL_LENGTH(N'dbo.Product', N'Quantity') IS NULL
BEGIN
    ALTER TABLE Product
    ADD Quantity INT NULL;
END
GO

UPDATE Product 
SET Quantity = 0 
WHERE Quantity IS NULL;
GO

ALTER TABLE Product
ALTER COLUMN Quantity INT NOT NULL;
GO

ALTER TABLE [dbo].[User]
ALTER COLUMN [Password] NVARCHAR(255) NOT NULL;
GO

UPDATE [dbo].[User]
SET [Password] = RTRIM([Password]);
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [Username] = N'admin')
    INSERT INTO [dbo].[User] ([Username], [Password], [UserRole]) VALUES (N'admin', N'123456', N'A');

IF NOT EXISTS (SELECT 1 FROM [dbo].[User] WHERE [Username] = N'khachhang1')
    INSERT INTO [dbo].[User] ([Username], [Password], [UserRole]) VALUES (N'khachhang1', N'123456', N'C');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Customer] WHERE [Username] = N'khachhang1')
    INSERT INTO [dbo].[Customer] ([CustomerName], [CustomerPhone], [CustomerEmail], [CustomerAddress], [Username])
    VALUES (N'Khách Hàng 1', N'0901234567', N'khachhang1@gmail.com', N'TP. Hồ Chí Minh', N'khachhang1');
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [CategoryName] = N'Điện thoại')
    INSERT INTO [dbo].[Category] ([CategoryName]) VALUES (N'Điện thoại');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [CategoryName] = N'Laptop')
    INSERT INTO [dbo].[Category] ([CategoryName]) VALUES (N'Laptop');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [CategoryName] = N'Phụ kiện')
    INSERT INTO [dbo].[Category] ([CategoryName]) VALUES (N'Phụ kiện');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Category] WHERE [CategoryName] = N'Sách')
    INSERT INTO [dbo].[Category] ([CategoryName]) VALUES (N'Sách');
GO

DELETE FROM [dbo].[Product]
WHERE [ProductName] NOT IN (
    N'Trí Tuệ Do Thái (Tái bản năm 2022)',
    N'iPhone 17 Pro Max 256GB Cam',
    N'Lịch sử sách',
    N'ASUS ROG Strix G16',
    N'MacBook Air M2 13 inch',
    N'Samsung Galaxy Book 6 Pro 16 inch (2026) Intel Core Ultra X7 358H, RAM 32GB, SSD 1TB, Intel Arc B390, 16" Dynamic AMOLED 2X(NS.B002)'
)
AND [ProductID] NOT IN (SELECT DISTINCT [ProductID] FROM [dbo].[OrderDetail]);
GO

DECLARE @CatPhone INT = (SELECT TOP 1 CategoryID FROM [dbo].[Category] WHERE CategoryName = N'Điện thoại');
DECLARE @CatLaptop INT = (SELECT TOP 1 CategoryID FROM [dbo].[Category] WHERE CategoryName = N'Laptop');
DECLARE @CatBook INT = (SELECT TOP 1 CategoryID FROM [dbo].[Category] WHERE CategoryName = N'Sách');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] = N'Trí Tuệ Do Thái (Tái bản năm 2022)')
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatBook, N'Trí Tuệ Do Thái (Tái bản năm 2022)', N'Cuốn sách khám phá bí quyết phát triển trí tuệ, khả năng tư duy và sáng tạo vượt trội của người Do Thái.', 179000, N'/Content/images/trituedothai.jpg', 50);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] = N'iPhone 17 Pro Max 256GB Cam')
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatPhone, N'iPhone 17 Pro Max 256GB Cam', N'Phiên bản màu Cam nổi bật, thiết kế cao cấp, hiệu năng mạnh mẽ và hệ thống camera chuyên nghiệp.', 32990000, N'/Content/images/iphone17.jpg', 15);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] = N'Lịch sử sách')
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatBook, N'Lịch sử sách', N'Tất cả chúng ta đều nghĩ mình biết một cuốn sách nghĩa là như thế nào. Nó có chữ, bìa và gáy. Nó cũng có thể chứa hình ảnh. Nếu là một tác phẩm tham khảo, nhiều khả năng nó sẽ bao gồm một danh sách nội dung cùng chỉ mục.', 329150, N'/Content/images/lichsusach.jpg', 98);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] = N'ASUS ROG Strix G16')
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatLaptop, N'ASUS ROG Strix G16', N'Laptop Gaming hiệu năng đỉnh cao cho game thủ và đồ họa.', 32990000, N'/Content/images/2.png', 5);

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] = N'MacBook Air M2 13 inch')
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatLaptop, N'MacBook Air M2 13 inch', N'Mỏng nhẹ, chip Apple M2 hiệu năng cao, pin lên đến 18 giờ.', 24500000, N'/Content/images/5.png', 8);

DECLARE @AsusImg NVARCHAR(MAX) = (SELECT TOP 1 [ProductImage] FROM [dbo].[Product] WHERE [ProductName] = N'ASUS ROG Strix G16');

IF NOT EXISTS (SELECT 1 FROM [dbo].[Product] WHERE [ProductName] LIKE N'Samsung Galaxy Book 6 Pro 16 inch%')
BEGIN
    INSERT INTO [dbo].[Product] ([CategoryID], [ProductName], [ProductDescription], [ProductPrice], [ProductImage], [Quantity]) 
    VALUES (@CatLaptop, N'Samsung Galaxy Book 6 Pro 16 inch (2026) Intel Core Ultra X7 358H, RAM 32GB, SSD 1TB, Intel Arc B390, 16" Dynamic AMOLED 2X(NS.B002)', N'Laptop cao cấp màn hình 16 inch Dynamic AMOLED 2X sắc nét, cấu hình Intel Core Ultra X7 mạnh mẽ, RAM 32GB và SSD 1TB.', 36890000, @AsusImg, 10);
END
ELSE
BEGIN
    UPDATE [dbo].[Product]
    SET [ProductImage] = @AsusImg
    WHERE [ProductName] LIKE N'Samsung Galaxy Book 6 Pro 16 inch%';
END
GO
