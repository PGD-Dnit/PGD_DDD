[33mcommit f453f9c783adae771e288ef5e4aac632621f201f[m[33m ([m[1;36mHEAD -> [m[1;32mmaster[m[33m)[m
Author: Clayton Silva e Araújo <clayton.araujo@dnit.gov.br>
Date:   Mon Apr 10 15:11:31 2023 -0300

    alteração p/ notificações danger

[1mdiff --git a/PGD.UI.Mvc/Controllers/PactoController.cs b/PGD.UI.Mvc/Controllers/PactoController.cs[m
[1mindex b7000d6..da0a050 100644[m
[1m--- a/PGD.UI.Mvc/Controllers/PactoController.cs[m
[1m+++ b/PGD.UI.Mvc/Controllers/PactoController.cs[m
[36m@@ -221,12 +221,35 @@[m [mnamespace PGD.UI.Mvc.Controllers[m
                 //VER AQUI COMO FAZER UM SELECT P PEGAR A COORDENAÇÃO E E SUAS SUBORDINADAS VER SELECT EM PACTOREPOSITORY LINHA 99[m
                 //unidades = _unidadeService.ObterUnidades().Where(x => x.IdUnidade == IdUnidade).ToList();         [m
                 //csa[m
[31m-                var unidadesSubordinadas = _unidadeService.ObterUnidadesSubordinadas(IdUnidadeSuperior).ToList();                [m
[31m-                unidades = unidadesSubordinadas;[m
[32m+[m[32m                var unidadesSubordinadas = _unidadeService.ObterUnidadesSubordinadasProcedure(IdUnidadeSuperior).ToList();[m[41m                [m
[32m+[m[32m                //unidades = (List<Unidade>)unidadesSubordinadas;[m
[32m+[m
[32m+[m[32m               // List<string> listaDeStrings = ObterListaDeStrings();[m
[32m+[m[32m                List<Unidade> listaDeUnidades = new List<Unidade>();[m
[32m+[m
[32m+[m[32m                foreach (var unid in unidadesSubordinadas)[m
[32m+[m[32m                {[m
[32m+[m[32m                    Unidade unidade = new Unidade[m
[32m+[m[32m                    {[m
[32m+[m[32m                        IdUnidade = unid.IdUnidade, // Atribua o valor da string à propriedade correspondente da classe Unidade[m
[32m+[m[32m                        Nome = unid.Nome,[m
[32m+[m[32m                        //Sigla = unid.Sigla[m
[32m+[m[32m                };[m
[32m+[m
[32m+[m[32m                    listaDeUnidades.Add(unidade); // Adicione o objeto Unidade à lista de Unidades[m
[32m+[m[32m                }[m
[32m+[m[32m                unidades = listaDeUnidades;[m
[32m+[m
[32m+[m
[32m+[m
             }[m
             else[m
             {[m
                 unidades = _unidadeService.ObterUnidades().ToList();[m
[32m+[m[32m                unidades = _unidadeService.Buscar(new UnidadeFiltro[m
[32m+[m[32m                {[m
[32m+[m[32m                   // IdUsuario = user.IdUsuario[m
[32m+[m[32m                }).Lista ?? new List<Unidade>();[m
 [m
             }[m
 [m
[36m@@ -259,22 +282,25 @@[m [mnamespace PGD.UI.Mvc.Controllers[m
             if (user.IsSolicitante)[m
             {[m
                 PactoCompleto.Searchpacto.NomeServidor = user.Nome;[m
[31m-                //csa aqui tenho que melhorar a consulta p obter pactos por cpf e não obter todos e usar o where p filtar depois [m
[32m+[m[32m                //csa[m[41m [m
                 retorno = _Pactoservice.ObterTodos(pactoViewModel, obj.ObterPactosUnidadesSubordinadas)[m
[31m-                    .Where(x => x.CpfUsuario == user.CPF).OrderByDescending(s => s.IdPacto).Take(50).ToList();[m
[32m+[m[32m                    .Where(x => x.CpfUsuario == user.CPF).OrderByDescending(s => s.IdPacto).Take(500).ToList();[m
 [m
                 //PAREI AQUI                [m
                 //var pac = new PactoViewModel();[m
[31m-                //foreach (var pacto in retorno)[m
[31m-                //{[m
[31m-                   [m
[31m-                //    if (pacto.IdSituacaoPacto == (int)eSituacaoPacto.AIniciar && pacto.DataPrevistaInicio <= DateTime.Now)[m
[31m-                //    {[m
[32m+[m[32m                foreach (var pacto in retorno)[m
[32m+[m[32m                {[m
[32m+[m
[32m+[m[32m                    if (pacto.IdSituacaoPacto == (int)eSituacaoPacto.AIniciar && pacto.DataPrevistaInicio <= DateTime.Now)[m
[32m+[m[32m                    {[m
                 //        pac = _Pactoservice.AtualizarPactosAiniciar(pacto);[m
[31m-                //    }                  [m
[31m-                    [m
[31m-                //}[m
[31m-                [m
[32m+[m[32m                        var pactoRetorno = _Pactoservice.AtualizarStatus(pacto, user, eAcaoPacto.Iniciando);[m
[32m+[m[32m                        retorno = _Pactoservice.ObterTodos(pactoViewModel, obj.ObterPactosUnidadesSubordinadas)[m
[32m+[m[32m                        .Where(x => x.CpfUsuario == user.CPF).OrderByDescending(s => s.IdPacto).Take(500).ToList();[m
[32m+[m[32m                    }[m[41m                  [m
[32m+[m
[32m+[m[32m                }[m
[32m+[m
             }[m
             else if (user.IsDirigente)[m
             {[m
[36m@@ -328,6 +354,11 @@[m [mnamespace PGD.UI.Mvc.Controllers[m
                 pacto = pactoRetorno;[m
                 if (pactoRetorno.ValidationResult.IsValid)[m
                     return setMessageAndRedirect("Plano de Trabalho excluído com sucesso!", "Index");[m
[32m+[m[32m                //if (!_notificadorAppService.TratarNotificacaoPacto(pactoBuscado, user, oper))[m
[32m+[m[32m                //{[m
[32m+[m[32m                //    var mensagem = "Plano de trabalho suspenso com sucesso, mas não foi possível enviar e-mail para um ou mais interessados.";[m
[32m+[m[32m                //    setMessage(mensagem);[m
[32m+[m[32m                //}[m
             }[m
 [m
             return setMessageAndRedirect(pacto.ValidationResult.Erros, "Index");[m
[36m@@ -833,28 +864,30 @@[m [mnamespace PGD.UI.Mvc.Controllers[m
 [m
             var user = getUserLogado();[m
             //csa validação data prevista de inicio, não permitir ao solicitante criar planos com datas retorativas [m
[31m-            //if (user.IsSolicitante && pactoViewModel.CpfUsuarioDirigente == null && pactoViewModel.IdSituacaoPacto == (int)eSituacaoPacto.PendenteDeAssinatura) {[m
[31m-            [m
[31m-[m
[32m+[m[32m            //if (user.IsSolicitante && pactoViewModel.CpfUsuarioDirigente == null && pactoViewModel.IdSituacaoPacto == (int)eSituacaoPacto.PendenteDeAssinatura) {[m[41m                       [m
             if (user.IsSolicitante && pactoViewModel.CpfUsuarioDirigente == null) {[m
[31m-[m
[31m-                var resultadoValidarDataPrevistaInicio = _Pactoservice.ValidarDataPrevistaInicio(pactoViewModel.DataPrevistaInicio, Domain.Enums.Perfil.Solicitante);[m
[32m+[m[41m                [m
[32m+[m[32m                var resultadoValidarDataPrevistaInicio = _Pactoservice.ValidarDataPrevistaInicio(pactoViewModel.DataPrevistaInicio, Domain.Enums.Perfil.Solicitante);[m[41m [m
[32m+[m[41m                [m
                 if (resultadoValidarDataPrevistaInicio != null)[m
                 {                    [m
[32m+[m[32m                    TempData["ValidarDataPrevistaInicio"] = TipoMessage.danger;[m[41m                    [m
[32m+[m
                     return setMessageAndRedirect($"{resultadoValidarDataPrevistaInicio}",[m
[31m-                  "Solicitar",[m
[31m-                   new RouteValueDictionary { { "id", pactoViewModel.IdPacto }, { "idTipoPacto", pactoViewModel.IdTipoPacto.ToString()}[m
[31m-                   });[m
[32m+[m[32m                        "Solicitar",[m
[32m+[m[32m                        new RouteValueDictionary { { "id", pactoViewModel.IdPacto }, { "idTipoPacto", pactoViewModel.IdTipoPacto.ToString()}[m
[32m+[m[32m                        });[m[41m  [m
[32m+[m[41m                    [m
                 }[m
[31m-            }[m
[31m-            //csa PAREI AQUI[m
[32m+[m[32m            }[m[41m            [m
             if (user.IsDirigente) {[m
 [m
                 var resultadoValidarDataPrevistaInicio = _Pactoservice.ValidarDataPrevistaInicio(pactoViewModel.DataPrevistaInicio, Domain.Enums.Perfil.Dirigente);[m
 [m
[31m-[m
                 if (resultadoValidarDataPrevistaInicio != null)[m
                 {[m
[32m+[m[32m                    TempData["teste"] = TipoMessage.danger;[m
[32m+[m
                     return setMessageAndRedirect($"{resultadoValidarDataPrevistaInicio}",[m
                   "Solicitar",[m
                    new RouteValueDictionary { { "id", pactoViewModel.IdPacto }, { "idTipoPacto", pactoViewModel.IdTipoPacto.ToString()}[m
[1mdiff --git a/PGD.UI.Mvc/Views/Shared/_AlertaPartial.cshtml b/PGD.UI.Mvc/Views/Shared/_AlertaPartial.cshtml[m
[1mindex dfbaeb4..f2a928c 100644[m
[1m--- a/PGD.UI.Mvc/Views/Shared/_AlertaPartial.cshtml[m
[1m+++ b/PGD.UI.Mvc/Views/Shared/_AlertaPartial.cshtml[m
[36m@@ -39,10 +39,22 @@[m [melse if (!ViewData.ModelState.IsValid && ViewData.ModelState["eSistuacao"] == nu[m
     </div>[m
 }[m
 <div>[m
[31m-    @if (messageViewModel != null)[m
[32m+[m[32m    @if (messageViewModel != null && TempData["ValidarDataPrevistaInicio"] != null)[m
     {[m
[32m+[m[32m        messageViewModel.Tipo = (TipoMessage)TempData["ValidarDataPrevistaInicio"];[m
         <div id="page-content-wrapper" class="body-content">[m
[31m-            <div id="divAlerta" class="row alert alert-@messageViewModel.Tipo.ToString() fade in"><!--cccc-->[m
[32m+[m[32m            <div id="divAlerta" class="row alert alert-@messageViewModel.Tipo.ToString() fade in">[m
[32m+[m[32m                <button type="button" class="close" data-dismiss="alert" aria-hidden="true">[m
[32m+[m[32m                    &times;[m
[32m+[m[32m                </button>[m
[32m+[m[32m                @messageViewModel.Mensagem[m
[32m+[m[32m            </div>[m
[32m+[m[32m        </div>[m
[32m+[m[32m    }[m
[32m+[m[32m    else if (messageViewModel != null)[m
[32m+[m[32m    {[m
[32m+[m[32m        <div id="page-content-wrapper" class="body-content">[m
[32m+[m[32m            <div id="divAlerta" class="row alert alert-@messageViewModel.Tipo.ToString() fade in">[m
                 <button type="button" class="close" data-dismiss="alert" aria-hidden="true">[m
                     &times;[m
                 </button>[m
[1mdiff --git a/PGD.UI.Mvc/Views/Shared/_Layout.cshtml b/PGD.UI.Mvc/Views/Shared/_Layout.cshtml[m
[1mindex f830e78..3c50859 100644[m
[1m--- a/PGD.UI.Mvc/Views/Shared/_Layout.cshtml[m
[1m+++ b/PGD.UI.Mvc/Views/Shared/_Layout.cshtml[m
[36m@@ -395,9 +395,9 @@[m
                 window.onload = function () {[m
 [m
                     var urlAtual = window.location.href;[m
[31m-                    var urlConsultar = window.location.protocol + "//" + window.location.host + "/" + "Pacto";[m
[31m-                    var urlPropor = window.location.protocol + "//" + window.location.host + "/" + "Pacto";[m
[31m-                    if (urlAtual == urlConsultar) {[m
[32m+[m[32m                    var urlConsultar = window.location.protocol + "//" + window.location.host + "/pgd/" + "Pacto";[m
[32m+[m[32m                    var urlConsultarHomologacao = window.location.protocol + "//" + window.location.host + "/" + "Pacto";[m
[32m+[m[32m                    if (urlAtual == urlConsultar || urlAtual == urlConsultarHomologacao) {[m
                         var adicionar = document.getElementById("menuConsultarPactos");[m
                         adicionar.classList.add("menu-sublinhado");[m
                     } else {                       [m

[33mcommit d6381f589515f54dd5457efdeececd98bc43d6ff[m[33m ([m[1;31morigin/pgd_v14[m[33m)[m
Author: Clayton Silva e Araújo <clayton.araujo@dnit.gov.br>
Date:   Wed Feb 15 17:34:04 2023 -0300

    alteração logo novo gov e titulos dos formularios

[1mdiff --git a/PGD.Domain/Services/PactoService.cs b/PGD.Domain/Services/PactoService.cs[m
[1mindex fed8337..f8425f0 100644[m
[1m--- a/PGD.Domain/Services/PactoService.cs[m
[1m+++ b/PGD.Domain/Services/PactoService.cs[m
[36m@@ -1042,7 +1042,7 @@[m [mnamespace PGD.Domain.Services[m
             if (user == Domain.Enums.Perfil.Dirigente && DataPrevistaInicio < dataPrazoRetroativoChefe)[m
             {[m
                 //return new ValidationResult("true");[m
[31m-                return new ValidationResult("A Data Prevista de Início deve ter no máximo 90 dias retrotivos.");[m
[32m+[m[32m                return new ValidationResult("A Data Prevista de Início deve ter no máximo 90 dias retroativos.");[m
             }[m
 [m
             ////////////////[m
[36m@@ -1051,13 +1051,13 @@[m [mnamespace PGD.Domain.Services[m
             if (user == Domain.Enums.Perfil.Solicitante && DataPrevistaInicio > dataPrazoPosteriorSolicitante)[m
             {[m
                 //return new ValidationResult($"A data de {DataPrevistaInicio.ToString()} deve ser maior ou igual à {dataAtual.ToShortDateString()} .");[m
[31m-                //return new ValidationResult("true");[m
[31m-                return new ValidationResult("Não foi possível salvar/assinar o plano. Verifique a data de início, a data está muita a frente da data atual.");[m
[32m+[m[32m                //return new ValidationResult("true");[m[41m                [m
[32m+[m[32m                return new ValidationResult("Não foi possível salvar/assinar o plano. Verifique a data de início.");[m
             }[m
             if (user == Domain.Enums.Perfil.Dirigente && DataPrevistaInicio > dataPrazoPosteriorChefe)[m
             {[m
                 //return new ValidationResult("true");[m
[31m-                return new ValidationResult("A Data Prevista de Início deve ter no máximo 180 dias a frente da data atual.");[m
[32m+[m[32m                return new ValidationResult("A Data Prevista de Início deve ter no máximo 180 dias à frente da data atual.");[m
             }[m
             return null;[m
         }[m
[1mdiff --git a/PGD.UI.Mvc/Content/css/global.css b/PGD.UI.Mvc/Content/css/global.css[m
[1mindex 2282ccd..9c773a3 100644[m
[1m--- a/PGD.UI.Mvc/Content/css/global.css[m
[1m+++ b/PGD.UI.Mvc/Content/css/global.css[m
[36m@@ -793,6 +793,7 @@[m [msubmitLink:focus {[m
     .btn-acao {[m
         width: 150px;[m
     }[m
[32m+[m[41m    [m
 }[m
 [m
 [m
[1mdiff --git a/PGD.UI.Mvc/Content/css/login.css b/PGD.UI.Mvc/Content/css/login.css[m
[1mindex 3370982..f6a36cd 100644[m
[1m--- a/PGD.UI.Mvc/Content/css/login.css[m
[1m+++ b/PGD.UI.Mvc/Content/css/login.css[m
[36m@@ -101,9 +101,9 @@[m
 [m
 .imagem_footer {[m
     position: fixed;[m
[31m-    bottom: 25px;[m
[31m-    width: 500px;[m
[31m-    max-width: 100%;[m
[32m+[m[32m    bottom: 5px;[m
[32m+[m[32m    width: 700px;[m
[32m+[m[41m   [m
     right: 3%;[m
     /*left: 49.5%;[m
     /*position: fixed;[m
[36m@@ -266,9 +266,9 @@[m
 [m
 .imagem_footer_perfil {[m
     position: fixed;[m
[31m-    bottom: 25px;[m
[31m-    left: 70%;[m
[31m-    width: 500px;[m
[32m+[m[32m    bottom: 5px;[m
[32m+[m[32m    left: 60%;[m
[32m+[m[32m    width: 700px;[m
     max-width: 100%;[m
 }[m
 [m
[36m@@ -288,10 +288,10 @@[m
     }[m
 [m
     .imagem_footer_perfil {[m
[31m-        width: 500px;[m
[32m+[m[32m        width: 700px;[m
         max-width: 100%;[m
[31m-        left: 71%;[m
[31m-        bottom: 25px;[m
[32m+[m[32m        left: 60.5%;[m
[32m+[m[32m        bottom: 5px;[m
         /* width: 250px;[m
         max-width: 100%;[m
         left: 62%;[m
[36m@@ -301,12 +301,11 @@[m
 [m
 @media (max-width: 1921px) {[m
     .image-footer-select-unid {[m
[31m-        position: fixed;[m
[31m-        bottom: 0;[m
[31m-        left: 71%;[m
[31m-        width: 500px;[m
[32m+[m[32m        position: fixed;[m[41m       [m
[32m+[m[32m        left: 60.5%;[m
[32m+[m[32m        width: 700px;[m
         max-width: 100%;[m
[31m-        bottom: 25px;[m
[32m+[m[32m        bottom: 5px;[m
     }[m
 }[m
 .title-select-unid {[m
[36m@@ -317,9 +316,9 @@[m
 [m
     .imagem_footer {[m
         position: relative;[m
[31m-        /*width: 500px;*/[m
[31m-        left: 100%;[m
[31m-        top: 40px;[m
[32m+[m[32m        width: 308px;[m
[32m+[m[32m        left: 109%;[m
[32m+[m[32m        bottom: -60px;[m
     }[m
     .perfil_body {[m
         width: 100%;[m
[1mdiff --git a/PGD.UI.Mvc/Controllers/LoginController.cs b/PGD.UI.Mvc/Controllers/LoginController.cs[m
[1mindex 07e7a0c..c79d7a5 100644[m
[1m--- a/PGD.UI.Mvc/Controllers/LoginController.cs[m
[1m+++ b/PGD.UI.Mvc/Controllers/LoginController.cs[m
[36m@@ -65,10 +65,10 @@[m [mnamespace PGD.UI.Mvc.Controllers[m
                 //Aplicação publicada como "Homologacao"[m
                 //[m
                 //if usado p o ambiente de homologação[m
[31m-                if (ConfigurationManager.AppSettings["ambiente"] != "Homologacao")[m
[32m+[m[32m                //if (ConfigurationManager.AppSettings["ambiente"] != "Homologacao")[m
 [m
                 //if usado p o ambiente de produção[m
[31m-                //if (ConfigurationManager.AppSettings["ambiente"] != "Desenvolvimento")[m
[32m+[m[32m                if (ConfigurationManager.AppSettings["ambiente"] != "Desenvolvimento")[m
                 {[m
                     AutenticarLDAP(loginViewModel);[m
                 }[m
[1mdiff --git a/PGD.UI.Mvc/Imagens/0101.png b/PGD.UI.Mvc/Imagens/0101.png[m
[1mnew file mode 100644[m
[1mindex 0000000..3c7259a[m
Binary files /dev/null and b/PGD.UI.Mvc/Imagens/0101.png differ
[1mdiff --git a/PGD.UI.Mvc/Imagens/fundo.dnit_v5-delado.png b/PGD.UI.Mvc/Imagens/fundo.dnit_v5-delado.png[m
[1mnew file mode 100644[m
[1mindex 0000000..a418996[m
Binary files /dev/null and b/PGD.UI.Mvc/Imagens/fundo.dnit_v5-delado.png differ
[1mdiff --git a/PGD.UI.Mvc/Imagens/fundo.dnit_v5.png b/PGD.UI.Mvc/Imagens/fundo.dnit_v5.png[m
[1mindex 053a8a0..b1156cc 100644[m
Binary files a/PGD.UI.Mvc/Imagens/fundo.dnit_v5.png and b/PGD.UI.Mvc/Imagens/fundo.dnit_v5.png differ
[1mdiff --git a/PGD.UI.Mvc/Imagens/fundo.dnit_v5_backup-2.png b/PGD.UI.Mvc/Imagens/fundo.dnit_v5_backup-2.png[m
[1mnew file mode 100644[m
[1mindex 0000000..053a8a0[m
Binary files /dev/null and b/PGD.UI.Mvc/Imagens/fundo.dnit_v5_backup-2.png differ
[1mdiff --git a/PGD.UI.Mvc/Imagens/logo_temp.png b/PGD.UI.Mvc/Imagens/logo_temp.png[m
[1mindex 4e3fc5f..ea87514 100644[m
Binary files a/PGD.UI.Mvc/Imagens/logo_temp.png and b/PGD.UI.Mvc/Imagens/logo_temp.png differ
[1mdiff --git a/PGD.UI.Mvc/Imagens/logo_temp_b (2).png b/PGD.UI.Mvc/Imagens/logo_temp_b (2).png[m
[1mnew file mode 100644[m
[1mindex 0000000..4e3fc5f[m
Binary files /dev/null and b/PGD.UI.Mvc/Imagens/logo_temp_b (2).png differ
[1mdiff --git a/PGD.UI.Mvc/Views/Login/Index.cshtml b/PGD.UI.Mvc/Views/Login/Index.cshtml[m
[1mindex 23676cf..d85b1d3 100644[m
[1m--- a/PGD.UI.Mvc/Views/Login/Index.cshtml[m
[1m+++ b/PGD.UI.Mvc/Views/Login/Index.cshtml[m
[36m@@ -3,64 +3,67 @@[m
     ViewBag.Title = "Login";[m
 }[m
 [m
[31m-<div class="left" style="background-color: red;"></div>[m
[32m+[m[32m<div class="left"></div>[m
 [m
[31m-<div class="right" style="background-color:red;">[m
[31m-    <h1 style="background-color:white; width:500px;">&nbsp&nbsp&nbsp--Ambiente de Desenvolvimento--</h1>[m
[31m-    <div class="col-md-8 col-sm-8 col-lg-8 col-xs-8" style="display: flex; justify-content: center; height: 110px; width: 500px">[m
[31m-        <img class="login-img" src="~/Imagens/fundo.dnit_v4.png" />[m
[31m-    </div>[m
[32m+[m[32m<div class="right">[m
[32m+[m[32m    <div class="col-md-12 c