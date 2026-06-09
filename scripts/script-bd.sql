-- EcoWatch - Oracle SQL Script
-- Generated from EF Core Migrations (InitialCreate -> AddNovosCamposOcorrencia)
-- Run this script to initialize the database schema from scratch.

-- ============================================================
-- TABLE: USUARIOS
-- ============================================================
CREATE TABLE "USUARIOS" (
    "Id"              RAW(16)          NOT NULL,
    "Nome"            NVARCHAR2(100)   NOT NULL,
    "Email"           NVARCHAR2(150)   NOT NULL,
    "SenhaHash"       NVARCHAR2(2000)  NOT NULL,
    "Role"            NVARCHAR2(2000)  NOT NULL,
    "DataCadastroUtc" TIMESTAMP(7)     NOT NULL,
    "Localidade"      NVARCHAR2(2000)  NULL,
    "RaioAlertasKm"   NUMBER(10)       DEFAULT 0 NOT NULL,
    CONSTRAINT "PK_USUARIOS" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_USUARIOS_Email" ON "USUARIOS" ("Email");

-- ============================================================
-- TABLE: OCORRENCIAS
-- ============================================================
CREATE TABLE "OCORRENCIAS" (
    "Id"                 RAW(16)          NOT NULL,
    "Latitude"           BINARY_DOUBLE    NOT NULL,
    "Longitude"          BINARY_DOUBLE    NOT NULL,
    "TipoOcorrencia"     NVARCHAR2(50)    NOT NULL,
    "DetalhesAdicionais" NVARCHAR2(2000)  NOT NULL,
    "DataOcorrenciaUtc"  TIMESTAMP(7)     NOT NULL,
    "UsuarioId"          RAW(16)          DEFAULT HEXTORAW('00000000000000000000000000000000') NOT NULL,
    "Area"               BINARY_DOUBLE    NULL,
    "Distancia"          BINARY_DOUBLE    NULL,
    "Urgencia"           NVARCHAR2(2000)  NULL,
    CONSTRAINT "PK_OCORRENCIAS" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_OCORRENCIAS_USUARIOS_UsuarioId"
        FOREIGN KEY ("UsuarioId") REFERENCES "USUARIOS" ("Id")
);

CREATE INDEX "IX_OCORRENCIAS_UsuarioId" ON "OCORRENCIAS" ("UsuarioId");
