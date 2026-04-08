CREATE TABLE Usuarios (
    Cpf VARCHAR(14) PRIMARY KEY,
    Nome VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL
);

CREATE TABLE Eventos (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Nome VARCHAR(150) NOT NULL,
    CapacidadeTotal INT NOT NULL,
    DataEvento DATETIME NOT NULL,
    PrecoPadrao DECIMAL(10,2) NOT NULL
);

CREATE TABLE Cupons (
    Codigo VARCHAR(50) PRIMARY KEY,
    PorcentagemDesconto DECIMAL(5,2) NOT NULL,
    ValorMinimoRegra DECIMAL(10,2) NOT NULL
);

CREATE TABLE Reservas (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioCpf VARCHAR(14) NOT NULL,
    EventoId INT NOT NULL,
    CupomUtilizado VARCHAR(50) NULL,
    ValorFinalPago DECIMAL(10,2) NOT NULL,

    CONSTRAINT FK_Reservas_Usuarios
        FOREIGN KEY (UsuarioCpf) REFERENCES Usuarios(Cpf),

    CONSTRAINT FK_Reservas_Eventos
        FOREIGN KEY (EventoId) REFERENCES Eventos(Id),

    CONSTRAINT FK_Reservas_Cupons
        FOREIGN KEY (CupomUtilizado) REFERENCES Cupons(Codigo)
);