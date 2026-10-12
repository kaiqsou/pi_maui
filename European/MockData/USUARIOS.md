# Usuários de exemplo (Mock)

Contas de teste semeadas em `MockDataService`. Use estas credenciais para entrar no app pela tela de **Login**.

| Nome             | E-mail                          | Senha           | Perfil        |
| ---------------- | ------------------------------- | --------------- | ------------- |
| Admin European   | admin@european.com              | admin1234       | Administrador |
| Marina Frankin   | marina.frankin@european.com     | professor1234   | Professor     |
| Kaique Souza     | kaique.souza@european.com       | aluno1234       | Aluno         |
| Ana Lima         | ana.lima@european.com           | aluna1234       | Aluno         |

## Regras de senha

A senha é validada no login (comparada ao hash salvo em `Usuario.SenhaHash`) e no cadastro, com estas regras simples:

- Mínimo de **6 caracteres**.
- Deve conter **letras e números**.
- **Sem espaços**.

## Observações

- As senhas são armazenadas como **PBKDF2 (SHA-256)** com **salt aleatório** e 100.000 iterações (`SenhaService.GerarHash`); não ficam em texto puro. Como o salt é aleatório, a mesma senha gera um hash diferente a cada vez.
- A verificação (`SenhaService.Verificar`) usa comparação em tempo constante; contas antigas em texto puro ou no SHA-256 antigo continuam funcionando.
- Usuários criados pelo app (a conta só é criada na confirmação do pagamento) entram como **Aluno**.
- Contas semeadas têm o hash recriado a cada inicialização (`GarantirUsuariosSemente`), então as senhas acima sempre funcionam mesmo com dados persistidos antigos.
- Ao cadastrar uma nova conta, a senha escolhida também precisa seguir as regras acima.
