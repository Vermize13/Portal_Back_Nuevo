-- Script para insertar roles iniciales en la base de datos
-- Ejecutar este script después de crear la base de datos con las migraciones

-- Nota: Los IDs son UUIDs generados. Cambiar si es necesario.

-- Insertar roles si no existen
INSERT INTO "Roles" ("Id", "Code", "Name", "Description")
VALUES 
    ('a0000000-0000-0000-0000-000000000001', 'Admin', 'Administrador', 'Administrador del sistema con acceso total'),
    ('a0000000-0000-0000-0000-000000000002', 'ProductOwner', 'Product Owner', 'Product Owner con permisos de gestión de proyectos'),
    ('a0000000-0000-0000-0000-000000000003', 'Developer', 'Desarrollador', 'Desarrollador con permisos básicos de desarrollo'),
    ('a0000000-0000-0000-0000-000000000004', 'Tester', 'Tester', 'Tester con permisos de gestión de incidencias y pruebas')
ON CONFLICT ("Id") DO NOTHING;

-- Verificar inserción
SELECT * FROM "Roles";
