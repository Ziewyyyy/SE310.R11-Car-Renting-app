-- CarRentalAPI demo dataset | PostgreSQL | EF Core 9
-- Requires both InitialCreate and AddRemainingTables migrations.
-- All identities and records are fictional; DO NOT use for production.
-- Run against the intended CarRentalDB database (not 'postgres').
-- Idempotent: deterministic UUIDs + ON CONFLICT DO NOTHING.
-- User PasswordHash intentionally contains an invalid sentinel; demo accounts cannot log in.
-- RefreshTokens intentionally left empty for security.
BEGIN;
SET LOCAL TIME ZONE 'UTC';

-- CarBrands: 7 rows
INSERT INTO "CarBrands" ("Id", "Name", "Country") VALUES
  ('31b7a12b-3fd3-52d8-8345-1befbcb3ec48', 'Toyota', 'Japan'),
  ('7eea0a9b-6414-57a6-95ac-7e4f992c275d', 'Honda', 'Japan'),
  ('21aeb247-b4c6-5e6e-a446-0cbe93f9daf2', 'Hyundai', 'South Korea'),
  ('899a3e29-0312-59e0-b7f7-11168e6bf2f0', 'Kia', 'South Korea'),
  ('dec20393-0909-5243-96e3-c96b3f04e09b', 'Mazda', 'Japan'),
  ('284a7249-ddb3-59bd-b4a6-21b18279d4f5', 'Ford', 'United States'),
  ('776e2316-03f3-5e73-8b96-adaf717a4fd4', 'Mitsubishi', 'Japan')
ON CONFLICT DO NOTHING;

-- CarCategories: 5 rows
INSERT INTO "CarCategories" ("Id", "Name", "Description") VALUES
  ('b85754ad-248a-5409-a320-6ddda0a106c2', 'Sedan', 'Xe 4-5 chỗ, phù hợp đi phố'),
  ('1a6f81a1-773a-5540-b7c9-d9a9bc461a0c', 'SUV', 'Xe gầm cao, phù hợp gia đình'),
  ('0325ed0b-e729-58e0-a6a8-46938a15e3be', 'MPV', 'Xe đa dụng 7 chỗ'),
  ('cff7f347-9818-56d2-8f50-fcc1fe9307de', 'Hatchback', 'Xe nhỏ gọn, dễ di chuyển'),
  ('61acacd4-9c77-5948-9e70-1850121a256a', 'Pickup', 'Xe bán tải')
ON CONFLICT DO NOTHING;

-- CarFeatures: 8 rows
INSERT INTO "CarFeatures" ("Id", "Name", "Description") VALUES
  ('a491f41c-1cc0-58fb-b410-bff5ea0c0a12', 'Điều hòa', 'Tiện ích điều hòa'),
  ('b4648101-ff14-5505-aec6-9eac9c06abf1', 'Bluetooth', 'Tiện ích bluetooth'),
  ('008eda8c-4630-52b7-b862-45e07c6e8eed', 'Camera lùi', 'Tiện ích camera lùi'),
  ('911867ce-229b-5039-af23-53461592492b', 'Cảm biến đỗ xe', 'Tiện ích cảm biến đỗ xe'),
  ('fc5dfda9-a94d-5a05-ac44-c24a03498543', 'Apple CarPlay / Android Auto', 'Tiện ích apple carplay / android auto'),
  ('4668c8be-447f-5dfe-b723-08d30403f196', 'Ghế da', 'Tiện ích ghế da'),
  ('4940045c-fbff-53d4-8b50-46cf9bc34edc', 'Định vị GPS', 'Tiện ích định vị gps'),
  ('4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279', 'Cruise Control', 'Tiện ích cruise control')
ON CONFLICT DO NOTHING;

-- Users: 8 rows
INSERT INTO "Users" ("Id", "FullName", "Email", "PasswordHash", "Phone", "Role") VALUES
  ('9a3086ed-5b08-5d5f-9b7e-d211f95d8163', 'Quản trị Demo', 'admin.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000001', 'Admin'),
  ('50c7f501-23d7-50a4-ba00-c2befe1d5246', 'Nguyễn Minh Anh', 'staff1.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000002', 'Staff'),
  ('140d95b3-f44d-50dc-9f74-0f1a3e917b14', 'Trần Hải Nam', 'staff2.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000003', 'Staff'),
  ('83ad3a16-b410-537f-85d2-6ad39dfdb893', 'Lê Thu Hà', 'khach1.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000004', 'Customer'),
  ('bec678a7-0c0f-532f-aa5e-6e68ed17a0fe', 'Phạm Quốc Bảo', 'khach2.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000005', 'Customer'),
  ('07681763-588d-5977-ac27-5aeb8b772bd9', 'Võ Ngọc Mai', 'khach3.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000006', 'Customer'),
  ('e94cb3d3-095f-57f2-b010-990e0aad651e', 'Đặng Thanh Bình', 'khach4.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000007', 'Customer'),
  ('21233132-e095-5abc-b680-6cc802ec9e2b', 'Bùi Hoàng Long', 'khach5.demo@carrental.test', '!DEMO_ACCOUNT_LOGIN_DISABLED!', '0900000008', 'Customer')
ON CONFLICT DO NOTHING;

-- Cars: 12 rows
INSERT INTO "Cars" ("Id", "Name", "LicensePlate", "ManufactureYear", "PricePerDay", "Color", "Seats", "Mileage", "Status", "CategoryId", "BrandId") VALUES
  ('f0a85a4b-f655-5bf5-870b-33ca43663ff5', 'Toyota Vios 2023', '51A-900.01', 2023, 650000, 'Trắng', 5, 23000, 'Available', 'b85754ad-248a-5409-a320-6ddda0a106c2', '31b7a12b-3fd3-52d8-8345-1befbcb3ec48'),
  ('84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', 'Honda City 2022', '51A-900.02', 2022, 700000, 'Đỏ', 5, 30500, 'Available', 'b85754ad-248a-5409-a320-6ddda0a106c2', '7eea0a9b-6414-57a6-95ac-7e4f992c275d'),
  ('c25b116b-ead5-5ca1-b106-8a04fad95ab0', 'Hyundai Accent 2023', '51A-900.03', 2023, 680000, 'Bạc', 5, 18000, 'Available', 'b85754ad-248a-5409-a320-6ddda0a106c2', '21aeb247-b4c6-5e6e-a446-0cbe93f9daf2'),
  ('b00a5c8a-70b9-5837-b18d-0c7519dd9726', 'Kia K3 2022', '51A-900.04', 2022, 750000, 'Xanh', 5, 27000, 'Available', 'b85754ad-248a-5409-a320-6ddda0a106c2', '899a3e29-0312-59e0-b7f7-11168e6bf2f0'),
  ('17435c3f-39e8-5682-b47f-2aac6300a219', 'Mazda CX-5 2023', '51A-900.05', 2023, 1050000, 'Đen', 5, 24000, 'Available', '1a6f81a1-773a-5540-b7c9-d9a9bc461a0c', 'dec20393-0909-5243-96e3-c96b3f04e09b'),
  ('bc157da7-2e6e-52cd-848b-d23d47b19783', 'Ford Everest 2022', '51A-900.06', 2022, 1550000, 'Trắng', 7, 41000, 'Available', '1a6f81a1-773a-5540-b7c9-d9a9bc461a0c', '284a7249-ddb3-59bd-b4a6-21b18279d4f5'),
  ('90785f63-cdbd-5d30-8c47-0fa423004e16', 'Mitsubishi Xforce 2024', '51A-900.07', 2024, 950000, 'Xám', 5, 9000, 'Available', '1a6f81a1-773a-5540-b7c9-d9a9bc461a0c', '776e2316-03f3-5e73-8b96-adaf717a4fd4'),
  ('280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 'Kia Seltos 2023', '51A-900.08', 2023, 980000, 'Đỏ', 5, 21000, 'Available', '1a6f81a1-773a-5540-b7c9-d9a9bc461a0c', '899a3e29-0312-59e0-b7f7-11168e6bf2f0'),
  ('1a01bc3d-57e2-543c-b9f7-70be9f055d6d', 'Mitsubishi Xpander 2023', '51A-900.09', 2023, 950000, 'Bạc', 7, 31000, 'Available', '0325ed0b-e729-58e0-a6a8-46938a15e3be', '776e2316-03f3-5e73-8b96-adaf717a4fd4'),
  ('d1ddd9f2-aa09-5759-b93b-74fe40412604', 'Kia Carnival 2022', '51A-900.10', 2022, 1800000, 'Đen', 7, 35000, 'Available', '0325ed0b-e729-58e0-a6a8-46938a15e3be', '899a3e29-0312-59e0-b7f7-11168e6bf2f0'),
  ('110d3be2-6258-590d-9870-3c5b603e1a11', 'Hyundai Grand i10 2022', '51A-900.11', 2022, 480000, 'Trắng', 5, 38000, 'Available', 'cff7f347-9818-56d2-8f50-fcc1fe9307de', '21aeb247-b4c6-5e6e-a446-0cbe93f9daf2'),
  ('b4b0399f-be56-5f8f-80db-f338a326f42e', 'Ford Ranger 2023', '51A-900.12', 2023, 1250000, 'Cam', 5, 19000, 'Available', '61acacd4-9c77-5948-9e70-1850121a256a', '284a7249-ddb3-59bd-b4a6-21b18279d4f5')
ON CONFLICT DO NOTHING;

-- CarImages: 12 rows
INSERT INTO "CarImages" ("Id", "CarId", "Url", "IsPrimary") VALUES
  ('f6778736-fcdd-5606-a423-99262ebea500', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', '/images/cars/vios.jpg', TRUE),
  ('c2100f4f-e57d-5481-acfe-5fbbaa66e651', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', '/images/cars/city.jpg', TRUE),
  ('d7ca2264-c407-5f33-abd2-5d24dbee08b8', 'c25b116b-ead5-5ca1-b106-8a04fad95ab0', '/images/cars/accent.jpg', TRUE),
  ('7668404a-988f-59e7-8429-720368dcb283', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', '/images/cars/k3.jpg', TRUE),
  ('1740912f-59e7-5885-ba33-534e0222620d', '17435c3f-39e8-5682-b47f-2aac6300a219', '/images/cars/cx5.jpg', TRUE),
  ('4820cc57-9c26-58b6-8b7c-92923bbe5d17', 'bc157da7-2e6e-52cd-848b-d23d47b19783', '/images/cars/everest.jpg', TRUE),
  ('96caa5d0-10c0-5f97-8b2b-55573f5d8ee6', '90785f63-cdbd-5d30-8c47-0fa423004e16', '/images/cars/xforce.jpg', TRUE),
  ('d5c7e25a-04ad-51dc-a0fa-6716869a2dd3', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', '/images/cars/seltos.jpg', TRUE),
  ('3c00a3f6-492c-5f3f-bc8e-4e9f26d4148c', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', '/images/cars/xpander.jpg', TRUE),
  ('7754a8b3-c494-5e50-9746-904a891f348a', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', '/images/cars/carnival.jpg', TRUE),
  ('4d14e143-6a8f-5e61-9e4c-3b7fa3b18d2d', '110d3be2-6258-590d-9870-3c5b603e1a11', '/images/cars/i10.jpg', TRUE),
  ('5b0c0511-c582-5f8b-a355-426205e8594d', 'b4b0399f-be56-5f8f-80db-f338a326f42e', '/images/cars/ranger.jpg', TRUE)
ON CONFLICT DO NOTHING;

-- CarFeatureMappings: 64 rows
INSERT INTO "CarFeatureMappings" ("Id", "CarId", "CarFeatureId") VALUES
  ('c2accfb5-c086-5c43-8f27-6910ed92ce74', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('8617cf70-fc24-5689-9581-072976f45b38', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('7a02c632-c53e-544c-97a7-9a0f9d380ab4', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('0a4849a9-4167-5cce-b941-c386f650e56b', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', '4940045c-fbff-53d4-8b50-46cf9bc34edc'),
  ('9c50a055-76fa-502c-86a3-2e4179996501', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('13a78b7a-1f59-55e9-bd8b-de63fd02f9d0', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('ac26225a-4704-5b24-b0ce-49262f9ebbe9', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('335115bc-3452-5ffa-9d9d-354a12cb6032', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', '4940045c-fbff-53d4-8b50-46cf9bc34edc'),
  ('413c99e8-cc3b-591b-8082-e684c0a55463', 'c25b116b-ead5-5ca1-b106-8a04fad95ab0', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('fffa8d27-363d-57ee-b1d3-8a75ee347eda', 'c25b116b-ead5-5ca1-b106-8a04fad95ab0', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('ac0d814b-fd29-581e-ae5c-1ebff2e323dd', 'c25b116b-ead5-5ca1-b106-8a04fad95ab0', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('8d49023a-37fe-5685-8ce9-c8cdd1bb541a', 'c25b116b-ead5-5ca1-b106-8a04fad95ab0', '4940045c-fbff-53d4-8b50-46cf9bc34edc'),
  ('1c1bb8db-df44-52c8-9763-78b65ce14bd5', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('e5474a89-db42-56e8-b215-ad2b21c744a9', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('ee63f444-afe2-589d-ae01-5038a962c309', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('edcdffa4-ef9a-5236-b2d7-a39775b2dcaf', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', '911867ce-229b-5039-af23-53461592492b'),
  ('b5991e2f-1836-5374-8199-ed79e87ed9d7', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('89e6b5b7-13ae-57bf-aebd-db653cbe3462', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('b001d6eb-c636-5f5c-a13a-6c967133fa3c', '17435c3f-39e8-5682-b47f-2aac6300a219', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('a6e0b121-94a7-5e1f-9331-27cb6f4df480', '17435c3f-39e8-5682-b47f-2aac6300a219', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('12e148ca-e113-5640-9866-42999a0365ed', '17435c3f-39e8-5682-b47f-2aac6300a219', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('afcd04ae-727f-531b-90ea-9cb8d131575a', '17435c3f-39e8-5682-b47f-2aac6300a219', '911867ce-229b-5039-af23-53461592492b'),
  ('630743d2-4dde-57c3-af6c-e7afe97e00f5', '17435c3f-39e8-5682-b47f-2aac6300a219', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('5512007d-d7c6-5f56-9386-bf2bc86eb8f7', '17435c3f-39e8-5682-b47f-2aac6300a219', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('88a0e402-a131-5cac-be0e-bbc536fde29b', 'bc157da7-2e6e-52cd-848b-d23d47b19783', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('8efa9d9b-2b89-52ee-bdc1-0ecad6af0da0', 'bc157da7-2e6e-52cd-848b-d23d47b19783', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('580111d1-7f30-5a1c-aea6-b58b1be3ba83', 'bc157da7-2e6e-52cd-848b-d23d47b19783', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('60c5a9bd-5f04-5998-ad51-db691339c85e', 'bc157da7-2e6e-52cd-848b-d23d47b19783', '911867ce-229b-5039-af23-53461592492b'),
  ('c667a4a7-b08b-5267-b98f-175cf48e6d9a', 'bc157da7-2e6e-52cd-848b-d23d47b19783', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('2bb1a4f8-7f4d-5fff-89fa-7136d7a92bd6', 'bc157da7-2e6e-52cd-848b-d23d47b19783', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('70bb9d41-93f9-55f3-ad41-8b741becd6f5', '90785f63-cdbd-5d30-8c47-0fa423004e16', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('21cd63e3-17c9-5c6e-bc5b-04d5960f8337', '90785f63-cdbd-5d30-8c47-0fa423004e16', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('af977e38-e71c-5b83-9c62-f511611e36d2', '90785f63-cdbd-5d30-8c47-0fa423004e16', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('7f1a1b23-472f-502f-a8a4-0e49e4ca31ac', '90785f63-cdbd-5d30-8c47-0fa423004e16', '911867ce-229b-5039-af23-53461592492b'),
  ('1915e73f-0e40-5925-a278-a997816614b9', '90785f63-cdbd-5d30-8c47-0fa423004e16', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('745909f6-8a39-5e91-b743-c31348f082fe', '90785f63-cdbd-5d30-8c47-0fa423004e16', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('c7c1e357-ab44-56ac-8ca5-1429ba188ecf', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('cfefb2e6-0a9e-504b-a556-7dd99fabfe2c', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('c9475bb3-929b-5314-bee7-c918a14e0a45', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('3177f4d9-a21f-5b12-9d6f-7be22f004fdb', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', '911867ce-229b-5039-af23-53461592492b'),
  ('45f0c584-d510-55a4-aea9-4466e951d26d', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('67097f60-91d1-534d-b3a7-29643998612e', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('9842896a-c038-54f0-81f9-82e6170143cd', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('4a192a99-a4d8-53b2-81e3-cd3485e2aca7', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('5601624a-2105-5ea1-b5b2-f6b430031bb2', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('614e858a-5c86-5787-8e43-186a32e269e2', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', '911867ce-229b-5039-af23-53461592492b'),
  ('e6f54499-8d7f-5d7c-ac90-042eab194df1', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('32923658-054c-5ccf-a272-d75e1e1b895d', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('9648ff19-2ce6-5520-ad88-d8f0906c37fe', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('8225ae02-4a57-58a0-b068-d807e5b4d6ce', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('1a547c4e-d50b-5f1c-bc88-a12b67da9b3f', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('9b07cd41-83a9-578a-9dec-d95550e320ee', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', '911867ce-229b-5039-af23-53461592492b'),
  ('f42557eb-d72c-578f-93ae-a85c797f0e0d', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('40ad52c6-64fa-536e-90fb-7b4f024ca5cb', 'd1ddd9f2-aa09-5759-b93b-74fe40412604', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279'),
  ('38f285e0-d6bd-53f7-b688-598f626e7802', '110d3be2-6258-590d-9870-3c5b603e1a11', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('953a093c-4dc6-505c-ab0f-b407598704f8', '110d3be2-6258-590d-9870-3c5b603e1a11', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('80cee435-5a42-5395-977a-a2a04b0141a4', '110d3be2-6258-590d-9870-3c5b603e1a11', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('b8a6f9c1-0a58-51a3-81eb-4e4ef5e24157', '110d3be2-6258-590d-9870-3c5b603e1a11', '4940045c-fbff-53d4-8b50-46cf9bc34edc'),
  ('9b529cff-3719-53fb-acbe-7c97a7173179', 'b4b0399f-be56-5f8f-80db-f338a326f42e', 'a491f41c-1cc0-58fb-b410-bff5ea0c0a12'),
  ('4e7e7298-1d92-59df-a62a-adba15b0ebb6', 'b4b0399f-be56-5f8f-80db-f338a326f42e', 'b4648101-ff14-5505-aec6-9eac9c06abf1'),
  ('97cc1d67-5f37-567c-9440-ce4d9fd0661f', 'b4b0399f-be56-5f8f-80db-f338a326f42e', '008eda8c-4630-52b7-b862-45e07c6e8eed'),
  ('89b6ab8e-49bc-5b6d-b4b5-aa6b88fb52f2', 'b4b0399f-be56-5f8f-80db-f338a326f42e', '911867ce-229b-5039-af23-53461592492b'),
  ('dd387be9-3270-5a5e-998b-161aa146d589', 'b4b0399f-be56-5f8f-80db-f338a326f42e', 'fc5dfda9-a94d-5a05-ac44-c24a03498543'),
  ('ea5d3609-0176-5c2f-a5f5-f078407e6411', 'b4b0399f-be56-5f8f-80db-f338a326f42e', '4ca90ad9-dba0-5c99-8a51-e2c8ddd8e279')
ON CONFLICT DO NOTHING;

-- Bookings: 8 rows
INSERT INTO "Bookings" ("Id", "CustomerId", "CarId", "PickupDate", "ReturnDate", "ActualReturnDate", "TotalAmount", "Status", "CreatedAt") VALUES
  ('dd770fae-d64e-5641-b94e-04148445afb8', '83ad3a16-b410-537f-85d2-6ad39dfdb893', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', '2026-11-02 08:00:00+00', '2026-11-05 08:00:00+00', NULL, 1950000, 'Confirmed', '2026-10-28 06:00:00+00'),
  ('39304236-38bc-599b-bc46-add1756958f7', 'bec678a7-0c0f-532f-aa5e-6e68ed17a0fe', '17435c3f-39e8-5682-b47f-2aac6300a219', '2026-11-06 08:00:00+00', '2026-11-08 08:00:00+00', NULL, 2100000, 'Pending', '2026-10-28 06:00:00+00'),
  ('8f8114f2-f4a7-5b55-9cc5-d5522766e33b', '07681763-588d-5977-ac27-5aeb8b772bd9', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', '2026-11-10 08:00:00+00', '2026-11-13 08:00:00+00', NULL, 2850000, 'Pending', '2026-10-28 06:00:00+00'),
  ('070755b5-35bb-5c58-859a-8928f7ae4876', 'e94cb3d3-095f-57f2-b010-990e0aad651e', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', '2026-10-10 08:00:00+00', '2026-10-12 08:00:00+00', '2026-10-12 08:00:00+00', 1400000, 'Completed', '2026-10-28 06:00:00+00'),
  ('810fbae5-5584-5e43-9242-d76f8232fad0', '21233132-e095-5abc-b680-6cc802ec9e2b', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', '2026-10-15 08:00:00+00', '2026-10-17 08:00:00+00', '2026-10-17 08:00:00+00', 1960000, 'Completed', '2026-10-28 06:00:00+00'),
  ('33b8f8ab-166f-5ca4-85df-0bd9854235ce', '83ad3a16-b410-537f-85d2-6ad39dfdb893', 'bc157da7-2e6e-52cd-848b-d23d47b19783', '2026-11-20 08:00:00+00', '2026-11-22 08:00:00+00', NULL, 3100000, 'Pending', '2026-10-28 06:00:00+00'),
  ('c966769a-7b5d-521b-9c56-b12518dc6b12', 'bec678a7-0c0f-532f-aa5e-6e68ed17a0fe', '110d3be2-6258-590d-9870-3c5b603e1a11', '2026-10-20 08:00:00+00', '2026-10-22 08:00:00+00', NULL, 960000, 'Cancelled', '2026-10-28 06:00:00+00'),
  ('3392671d-5544-5ed8-b708-33a253fac8d3', '07681763-588d-5977-ac27-5aeb8b772bd9', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', '2026-11-12 08:00:00+00', '2026-11-14 08:00:00+00', NULL, 1500000, 'Pending', '2026-10-28 06:00:00+00')
ON CONFLICT DO NOTHING;

-- BookingDetails: 8 rows
INSERT INTO "BookingDetails" ("Id", "BookingId", "CarId", "UnitPrice", "Amount") VALUES
  ('9f73774b-6c4b-54a3-885d-02376fc31f0e', 'dd770fae-d64e-5641-b94e-04148445afb8', 'f0a85a4b-f655-5bf5-870b-33ca43663ff5', 650000, 1950000),
  ('bdded465-874c-5056-a039-873c2e859891', '39304236-38bc-599b-bc46-add1756958f7', '17435c3f-39e8-5682-b47f-2aac6300a219', 1050000, 2100000),
  ('8d28a123-8cd0-57a5-a682-4f4b2143c4ab', '8f8114f2-f4a7-5b55-9cc5-d5522766e33b', '1a01bc3d-57e2-543c-b9f7-70be9f055d6d', 950000, 2850000),
  ('cab8beb2-5498-55b1-9d0c-7fe9a2ceb598', '070755b5-35bb-5c58-859a-8928f7ae4876', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', 700000, 1400000),
  ('bdf248b7-3366-5843-9315-e2ac088260bc', '810fbae5-5584-5e43-9242-d76f8232fad0', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 980000, 1960000),
  ('dce11237-e792-53aa-b90b-49df19bf00f4', '33b8f8ab-166f-5ca4-85df-0bd9854235ce', 'bc157da7-2e6e-52cd-848b-d23d47b19783', 1550000, 3100000),
  ('b91617dd-9f31-56bd-8291-a039ef2189c8', 'c966769a-7b5d-521b-9c56-b12518dc6b12', '110d3be2-6258-590d-9870-3c5b603e1a11', 480000, 960000),
  ('0e15c669-1621-5ad5-8317-4c3e8f35c55f', '3392671d-5544-5ed8-b708-33a253fac8d3', 'b00a5c8a-70b9-5837-b18d-0c7519dd9726', 750000, 1500000)
ON CONFLICT DO NOTHING;

-- Contracts: 3 rows
INSERT INTO "Contracts" ("Id", "BookingId", "SignedAt", "StartDate", "EndDate", "DepositAmount", "Status") VALUES
  ('7301aeab-83f2-5d2d-bc6e-35118bccf062', 'dd770fae-d64e-5641-b94e-04148445afb8', '2026-10-28 07:00:00+00', '2026-11-02 08:00:00+00', '2026-11-05 08:00:00+00', 300000, 'Active'),
  ('58993405-a4b4-5031-a91a-314f3c873faf', '070755b5-35bb-5c58-859a-8928f7ae4876', '2026-10-28 07:00:00+00', '2026-10-10 08:00:00+00', '2026-10-12 08:00:00+00', 200000, 'Completed'),
  ('08b5abd0-67ae-5efb-bee6-9dea3b7932b5', '810fbae5-5584-5e43-9242-d76f8232fad0', '2026-10-28 07:00:00+00', '2026-10-15 08:00:00+00', '2026-10-17 08:00:00+00', 300000, 'Completed')
ON CONFLICT DO NOTHING;

-- Payments: 3 rows
INSERT INTO "Payments" ("Id", "ContractId", "Amount", "PaidAt", "Method", "Status", "TransactionCode") VALUES
  ('95d48067-4b92-5f7e-a57b-28318b92c6a5', '7301aeab-83f2-5d2d-bc6e-35118bccf062', 300000, '2026-10-28 08:00:00+00', 'VNPay', 'Success', 'DEMO-VNPAY-B1'),
  ('6b7c48bc-d64e-5145-b7f2-d50874b2f551', '58993405-a4b4-5031-a91a-314f3c873faf', 1400000, '2026-10-28 08:00:00+00', 'VNPay', 'Success', 'DEMO-VNPAY-B4'),
  ('e339cda6-2abf-5bbe-aa9f-c9db535e7bff', '08b5abd0-67ae-5efb-bee6-9dea3b7932b5', 1960000, '2026-10-28 08:00:00+00', 'VNPay', 'Success', 'DEMO-VNPAY-B5')
ON CONFLICT DO NOTHING;

-- RentalExtensions: 1 rows
INSERT INTO "RentalExtensions" ("Id", "BookingId", "NewReturnDate", "ExtraAmount", "Status", "RequestedAt") VALUES
  ('5a743e62-3ef0-5880-bc18-5d93409a6a92', 'dd770fae-d64e-5641-b94e-04148445afb8', '2026-11-06 08:00:00+00', 650000, 'Pending', '2026-10-28 09:00:00+00')
ON CONFLICT DO NOTHING;

-- CarHandoverRecords: 2 rows
INSERT INTO "CarHandoverRecords" ("Id", "BookingId", "StaffId", "RecordType", "RecordedAt", "Mileage", "ConditionNotes") VALUES
  ('5667da7c-d353-5a5d-b9aa-858cb91340c6', '070755b5-35bb-5c58-859a-8928f7ae4876', '50c7f501-23d7-50a4-ba00-c2befe1d5246', 'Return', '2026-10-12 08:00:00+00', 30700, 'Xe sạch, đủ phụ kiện'),
  ('ae500f79-6478-5cca-aa6e-fad6a3355b6c', '810fbae5-5584-5e43-9242-d76f8232fad0', '140d95b3-f44d-50dc-9f74-0f1a3e917b14', 'Return', '2026-10-17 08:00:00+00', 21250, 'Không phát hiện hư hỏng')
ON CONFLICT DO NOTHING;

-- Reviews: 2 rows
INSERT INTO "Reviews" ("Id", "CustomerId", "CarId", "Rating", "Content", "CreatedAt") VALUES
  ('e09b830e-5d13-5275-8ab1-89e33a8a723d', 'e94cb3d3-095f-57f2-b010-990e0aad651e', '84d70aa3-1c7b-5a3e-9cf5-f24ae703ac53', 5, 'Xe sạch, giao nhận nhanh.', '2026-10-13 09:00:00+00'),
  ('285a53cd-e906-56eb-bb82-c694fb304e40', '21233132-e095-5abc-b680-6cc802ec9e2b', '280dd6eb-8106-566c-aadc-06bbbcdc4bc0', 4, 'Xe vận hành tốt, nội thất đẹp.', '2026-10-18 09:00:00+00')
ON CONFLICT DO NOTHING;

-- TestDriveBookings: 2 rows
INSERT INTO "TestDriveBookings" ("Id", "CustomerId", "CarId", "ScheduledAt", "Status") VALUES
  ('d466f2d6-cdee-57ec-a852-0585f3539011', 'bec678a7-0c0f-532f-aa5e-6e68ed17a0fe', '17435c3f-39e8-5682-b47f-2aac6300a219', '2026-11-03 03:00:00+00', 'Pending'),
  ('2425ab3b-2d6a-56a8-a438-a040279099ed', '07681763-588d-5977-ac27-5aeb8b772bd9', '90785f63-cdbd-5d30-8c47-0fa423004e16', '2026-11-04 03:00:00+00', 'Confirmed')
ON CONFLICT DO NOTHING;

-- SupportRequests: 2 rows
INSERT INTO "SupportRequests" ("Id", "CustomerId", "StaffId", "Title", "Content", "Status", "CreatedAt") VALUES
  ('8f429c8b-c93f-5285-85a0-39eeecbf3f92', '83ad3a16-b410-537f-85d2-6ad39dfdb893', '50c7f501-23d7-50a4-ba00-c2befe1d5246', 'Hỏi thủ tục nhận xe', 'Cần mang theo giấy tờ gì khi nhận xe?', 'InProgress', '2026-10-28 09:00:00+00'),
  ('d6f31564-bd50-5091-bc12-64c7a0f20a7e', '07681763-588d-5977-ac27-5aeb8b772bd9', NULL, 'Thay đổi giờ nhận xe', 'Tôi muốn nhận xe muộn hơn 1 giờ.', 'Open', '2026-10-28 10:00:00+00')
ON CONFLICT DO NOTHING;

-- Notifications: 2 rows
INSERT INTO "Notifications" ("Id", "UserId", "Content", "Type", "IsRead", "CreatedAt") VALUES
  ('5ce787a4-d72c-5d30-ac1c-d26154562857', '83ad3a16-b410-537f-85d2-6ad39dfdb893', 'Đơn thuê b1 đã được xác nhận.', 'Booking', FALSE, '2026-10-28 10:00:00+00'),
  ('73d66216-c800-55e7-8244-f44ce9a74604', '50c7f501-23d7-50a4-ba00-c2befe1d5246', 'Có yêu cầu hỗ trợ mới.', 'Support', FALSE, '2026-10-28 10:05:00+00')
ON CONFLICT DO NOTHING;

-- Reports: 1 rows
INSERT INTO "Reports" ("Id", "CreatedById", "ReportType", "Content", "CreatedAt") VALUES
  ('f1473114-dd85-5ea3-b41f-6f50949ad956', '9a3086ed-5b08-5d5f-9b7e-d211f95d8163', 'MonthlyRevenue', 'Dữ liệu minh họa; không phải báo cáo doanh thu thực.', '2026-10-28 10:00:00+00')
ON CONFLICT DO NOTHING;

-- Refunds: 1 rows
INSERT INTO "Refunds" ("Id", "PaymentId", "Amount", "Status", "Reason", "CreatedAt") VALUES
  ('83a363ae-8dcd-5cb9-b2d5-eac68143a1cf', '95d48067-4b92-5f7e-a57b-28318b92c6a5', 100000, 'Pending', 'Yêu cầu điều chỉnh cọc (demo)', '2026-10-28 10:00:00+00')
ON CONFLICT DO NOTHING;

-- AuditLogs: 2 rows
INSERT INTO "AuditLogs" ("Id", "ActorId", "Action", "EntityName", "EntityId", "CreatedAt") VALUES
  ('477105bc-ef82-522d-a167-de33044f3d65', '50c7f501-23d7-50a4-ba00-c2befe1d5246', 'CONFIRM_BOOKING', 'Booking', 'dd770fae-d64e-5641-b94e-04148445afb8', '2026-10-28 10:00:00+00'),
  ('74ff367e-dc8f-5f66-92a9-4886402d0cdc', '9a3086ed-5b08-5d5f-9b7e-d211f95d8163', 'CREATE_REPORT', 'Report', 'f1473114-dd85-5ea3-b41f-6f50949ad956', '2026-10-28 10:05:00+00')
ON CONFLICT DO NOTHING;

COMMIT;

-- Verification: table row counts (run separately if desired)
SELECT 'Users' AS table_name, COUNT(*) AS total FROM "Users"
UNION ALL SELECT 'Cars', COUNT(*) FROM "Cars"
UNION ALL SELECT 'Bookings', COUNT(*) FROM "Bookings"
UNION ALL SELECT 'Payments', COUNT(*) FROM "Payments"
UNION ALL SELECT 'CarImages', COUNT(*) FROM "CarImages";
