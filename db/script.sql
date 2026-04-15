CREATE TABLE usuarios (
    cpf VARCHAR(14) PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(100) NOT NULL
);

CREATE TABLE eventos (
    id SERIAL PRIMARY KEY,
    nome VARCHAR(150) NOT NULL,
    capacidadetotal INT NOT NULL,
    dataevento TIMESTAMP NOT NULL,
    precopadrao DECIMAL(10,2) NOT NULL,
    usuariocpf VARCHAR(14) NOT NULL,
    descricao TEXT NULL,
    local VARCHAR(200) NULL,
    imagemurl TEXT NULL,
    CONSTRAINT fk_eventos_usuarios FOREIGN KEY (usuariocpf) REFERENCES usuarios(cpf)
);

CREATE TABLE cupons (
    codigo VARCHAR(50) PRIMARY KEY,
    porcentagemdesconto DECIMAL(5,2) NOT NULL,
    valorminimoregra DECIMAL(10,2) NOT NULL
);

CREATE TABLE reservas (
    id SERIAL PRIMARY KEY,
    usuariocpf VARCHAR(14) NOT NULL,
    eventoid INT NOT NULL,
    cupomutilizado VARCHAR(50) NULL,
    valorfinalpago DECIMAL(10,2) NOT NULL,

    CONSTRAINT fk_reservas_usuarios
        FOREIGN KEY (usuariocpf) REFERENCES usuarios(cpf),

    CONSTRAINT fk_reservas_eventos
        FOREIGN KEY (eventoid) REFERENCES eventos(id),

    CONSTRAINT fk_reservas_cupons
        FOREIGN KEY (cupomutilizado) REFERENCES cupons(codigo)
);
