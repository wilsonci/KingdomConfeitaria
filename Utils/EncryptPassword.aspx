<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="EncryptPassword.aspx.cs" Inherits="KingdomConfeitaria.EncryptPassword" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Criptografar Senha - Kingdom Confeitaria</title>
    <link href="../Content/bootstrap/bootstrap.min.css" rel="stylesheet" />
</head>
<body>
    <div class="container mt-5">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="card">
                    <div class="card-header bg-primary text-white">
                        <h4 class="mb-0">Criptografar Senha do Gmail</h4>
                    </div>
                    <div class="card-body">
                        <form id="form1" runat="server">
                            <div class="mb-3">
                                <label for="txtSenha" class="form-label">Senha (texto plano):</label>
                                <asp:TextBox ID="txtSenha" runat="server" CssClass="form-control" TextMode="Password" placeholder="Digite a senha do Gmail"></asp:TextBox>
                                <small class="form-text text-muted">Digite a senha do Gmail que deseja criptografar</small>
                            </div>
                            
                            <div class="mb-3">
                                <asp:Button ID="btnCriptografar" runat="server" Text="Criptografar" CssClass="btn btn-primary" OnClick="btnCriptografar_Click" />
                            </div>
                            
                            <asp:Panel ID="divResultado" runat="server" Visible="false" CssClass="alert alert-success">
                                <h5>Senha Criptografada:</h5>
                                <div class="mb-2">
                                    <strong>Texto criptografado:</strong>
                                    <div class="p-2 bg-light border rounded">
                                        <asp:Literal ID="txtCriptografado" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div class="mb-2">
                                    <strong>Para usar no Web.config:</strong>
                                    <div class="p-2 bg-light border rounded">
                                        <code>&lt;add key="SmtpPassword" value="<asp:Literal ID="litConfig" runat="server"></asp:Literal>" /&gt;</code>
                                    </div>
                                </div>
                                <div class="mt-3">
                                    <button type="button" class="btn btn-sm btn-outline-secondary" onclick="copiarTexto()">Copiar Texto Criptografado</button>
                                </div>
                            </asp:Panel>
                            
                            <asp:Panel ID="divErro" runat="server" Visible="false" CssClass="alert alert-danger">
                                <asp:Literal ID="litErro" runat="server"></asp:Literal>
                            </asp:Panel>
                        </form>
                    </div>
                </div>
                
                <div class="card mt-3">
                    <div class="card-body">
                        <h5>Instruções:</h5>
                        <ol>
                            <li>Digite a senha do Gmail no campo acima</li>
                            <li>Clique em "Criptografar"</li>
                            <li>Copie o texto criptografado gerado</li>
                            <li>Cole no arquivo Web.config na chave "SmtpPassword"</li>
                            <li>Remova este arquivo (EncryptPassword.aspx) após usar</li>
                        </ol>
                        <div class="alert alert-warning">
                            <strong>Atenção:</strong> Este utilitário deve ser removido após o uso por questões de segurança.
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
    
    <script>
        function copiarTexto() {
            var texto = document.getElementById('<%= txtCriptografado.ClientID %>').innerText || document.getElementById('<%= txtCriptografado.ClientID %>').textContent;
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(texto).then(function() {
                    alert('Texto copiado para a área de transferência!');
                }).catch(function() {
                    // Fallback para navegadores mais antigos
                    var textArea = document.createElement("textarea");
                    textArea.value = texto;
                    document.body.appendChild(textArea);
                    textArea.select();
                    document.execCommand('copy');
                    document.body.removeChild(textArea);
                    alert('Texto copiado para a área de transferência!');
                });
            } else {
                // Fallback para navegadores mais antigos
                var textArea = document.createElement("textarea");
                textArea.value = texto;
                document.body.appendChild(textArea);
                textArea.select();
                document.execCommand('copy');
                document.body.removeChild(textArea);
                alert('Texto copiado para a área de transferência!');
            }
        }
    </script>
</body>
</html>

