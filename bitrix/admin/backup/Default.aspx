<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="Backup.aspx.cs" Inherits="Backup" %>

<%@ Import Namespace="Bitrix.Services" %>
<%@ Import Namespace="Bitrix.DataTypes" %>
<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <bx:BXTabControl ID="BXTabControl1" ValidationGroup="vgInnerForm" runat="server"
        OnCommand="BXTabControl1_Command" ButtonsMode="Hidden">
        <Tabs>
            <bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="Модуль для резервного копирования"
                Title="<%$ LocRaw:TabTitle.Main %>">






<div class="notes">
<table cellspacing="0" cellpadding="0" border="0" class="notes" width="100%">
	<tbody><tr class="top">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr>
		<td class="left"><div class="empty"></div></td>
		<td class="content">Для переноса архива сайта на другой хостинг поместите в корневой папке нового сайта содержимое <a href="/bitrix/admin/backup/RestoreTool.rar">пакета восстановления</a> и сам архив, затем наберите в строке браузера "<имя сайта>/Restore.aspx" и следуйте инструкциям по распаковке.<br>
		</td>
		<td class="right"><div class="empty"></div></td>
	</tr>
	<tr class="bottom">
		<td class="left"><div class="empty"></div></td>
		<td><div class="empty"></div></td>
		<td class="right"><div class="empty"></div></td>
	</tr>
</tbody></table>
</div>







                <div>
                    <table border="0" cellpadding="0" cellspacing="0" class="edit-table">
                        <tr class="heading">
                            <td colspan="2">
                                Файлы
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Архивировать ядро:
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbArchiveCore" runat="server" Checked="True" Enabled="false" />
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Архивировать публичную часть:
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbArchivePublicPart" runat="server" Checked="True" />
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Исключить из архива файлы размером более (0 - без ограничения):
                            </td>
                            <td width="50%">
                                <asp:TextBox ID="txbExcludeFileSize" ValidationGroup="vgInnerForm" runat="server"
                                    Width="60px" Text="0"></asp:TextBox>&nbsp;кб
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Исключить из архива файлы и директории по маске:
                            </td>
                            <td width="50%">
                                <asp:TextBox ID="txbExcludeFiles" runat="server" placeholder="*.jpg;*/.svn;*/.svn/*"
                                    Width="420px" />
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Не сжимать файлы:
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbExcludeStandardTypesCompressing" runat="server" />&nbsp;jpg,
                                jpeg, mp3, 7z, bz2, cab, gz, lha, lzh, rar, taz, tgz, z, zip, ace, arj
                            </td>
                        </tr>
                        <tr class="heading">
                            <td colspan="2">
                                База данных
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Архивировать базу данных:
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbIncludeDataBase" runat="server" />
                            </td>
                        </tr>
                        <tr class="heading">
                            <td colspan="2">
                                Серверные ограничения
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Длительность шага:
                            </td>
                            <td width="50%">
                                <asp:TextBox ID="txbStepDuration" runat="server" Text="20" Width="40px"></asp:TextBox>&nbsp;сек.,
                                интервал:
                                <asp:TextBox ID="txbSleep" runat="server" Text="5" Width="40px"></asp:TextBox>&nbsp;сек.
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Отключить компрессию архива (снижение нагрузки на процессор):
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbDisableCompressing" runat="server" />
                            </td>
                        </tr>
                        <tr valign="top">
                            <td class="field-name" width="50%">
                                Проверить целосность архива после завершения:
                            </td>
                            <td width="50%">
                                <asp:CheckBox ID="chbCheckPackage" runat="server" />
                            </td>
                        </tr>
                        <tr class="heading">
                            <td colspan="2">
                                Агент резервного копирования
                            </td>
                        </tr>
                        <tr>
                            <td align="right" colspan="2">
                                <asp:UpdatePanel ID="AgentUpdatePanel" runat="server">
                                    <ContentTemplate>
                                        <bx:BXPopupPanel ID="AgentPopupPanel" runat="server">
                                            <Commands>
                                                <bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteTask %>"
                                                    OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
                                            </Commands>
                                        </bx:BXPopupPanel>
                                        <bx:BXGridView ID="AgentGridView" runat="server" ContentName="" AllowSorting="True"
                                            PopupCommandMenuId="AgentPopupPanel" DataSourceID="AgentGridView" AllowPaging="True"
                                            AjaxConfiguration-UpdatePanelId="AgentUpdatePanel" OnSelect="AgentGridView_Select"
                                            OnSelectCount="AgentGridView_SelectCount" OnDelete="AgentGridView_Delete" DataKeyNames="ID">
                                            <Columns>
                                                <asp:BoundField DataField="Id" HeaderText="ID" ReadOnly="True" SortExpression="Id" />
                                                <asp:BoundField DataField="Name" HeaderText="Имя" ReadOnly="True" SortExpression="Name" />
                                                <asp:BoundField DataField="StartTime" HeaderText="Время начала" ReadOnly="True" SortExpression="StartTime" DataFormatString="{0:G}"/>
                                                <asp:BoundField DataField="Period" HeaderText="Период" ReadOnly="True" SortExpression="Period" />
                                            </Columns>
                                            <AjaxConfiguration UpdatePanelId="AgentUpdatePanel" />
                                        </bx:BXGridView>
                                        <%--<bx:BXMultiActionMenuToolbar ID="AgentMultiActionMenuToolbar" runat="server" >
                                            <Items>
                                                <bx:BXMamImageButton ID="BXMamImageButton2" runat="server" ShowConfirmDialog="true" 
                                                    ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>"
                                                    EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
                                                    CommandName="delete" />
                                            </Items>
                                        </bx:BXMultiActionMenuToolbar>--%>
                                    </ContentTemplate>
                                </asp:UpdatePanel>
                            </td>
                        </tr>
                        <tr>
                            <td align="center" colspan="2">
                                <img id="progress" src="/bitrix/images/update_progressbar.gif" style="display: none" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                                <div id="box" class="buttons" style="display: none; z-index: 10; top: 40%; left: 40%;
                                    border-width: thin; border-color: #E0E4F1; position: fixed">
                                    <table border="0" cellpadding="0" cellspacing="0">
                                        <tr>
                                            <td class="field-name">
                                                Имя агента
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txbAgentName" runat="server" placeHolder="Агента 1"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="field-name">
                                                Интервал в часах
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txbHour" runat="server"  placeHolder="24"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="field-name">
                                                Время начала
                                            </td>
                                            <td>
                                                <asp:TextBox ID="txbStartTime" runat="server" placeHolder="23:59"/>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td>
                                                <asp:Button ID="btnCreateAgent" runat="server" OnClick="btnCreateAgent_Click" Text="Создать" />
                                                <input type="button" onclick="javascript: document.getElementById('box').style.display = 'none';"
                                                    value="Отменить" />
                                            </td>
                                        </tr>
                                    </table>
                                </div>
                            </td>
                        </tr>
                    </table>
                </div>
            </bx:BXTabControlTab>
        </Tabs>
        <ButtonsBar runat="server" ID="l">
            <asp:Button ID="btnArchive" runat="server" OnClick="btnArchive_Click" Text="Архивировать"
                OnClientClick="javascript:document.getElementById('progress').style.display='block'" />
            <input type="button" value="Создать агент с выбранными конфигурациями" onclick="showPopup()" />
            <asp:Label ID="lblError" runat="server" ForeColor="Red"></asp:Label>
        </ButtonsBar>
    </bx:BXTabControl>

    <script>
        function showPopup() {
            document.getElementById('box').style.display = 'block';
        }
    </script>

    <table style="width: 100%">
        <tr>
            <td align="right">
                <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                    <ContentTemplate>
                        <bx:BXPopupPanel ID="BackupsPopupPanel" runat="server">
                            <Commands>
                                <bx:CommandItem IconClass="delete" ItemText="<%$ Loc:Kernel.Delete %>" ItemTitle="<%$ Loc:PopupTitle.DeleteTask %>"
                                    OnClickScript="" UserCommandId="delete" ShowConfirmDialog="true" ConfirmDialogText="<%$ LocRaw:PopupConfirmDialogText.Delete %>" />
                            </Commands>
                        </bx:BXPopupPanel>
                        <bx:BXGridView ID="BackupsGridView" runat="server" ContentName="" AllowSorting="True"
                            ContextMenuToolbarId="BackupsMultiActionMenuToolbar" PopupCommandMenuId="BackupsPopupPanel"
                            DataSourceID="BackupsGridView" AllowPaging="True" AjaxConfiguration-UpdatePanelId="UpdatePanel1"
                            OnSelect="BackupsGridView_Select" OnSelectCount="BackupsGridView_SelectCount"
                            OnDelete="BackupsGridView_Delete" DataKeyNames="FullName">
                            <Columns>
                                <asp:HyperLinkField DataNavigateUrlFields="Name" DataNavigateUrlFormatString="~/backup/{0}"
                                    DataTextField="Name" HeaderText="Имя" />
                                <asp:BoundField HeaderText="Размер файла" DataField="Size" DataFormatString="{0} Мб" />
                                <asp:BoundField HeaderText="Изменен" DataField="Created" DataFormatString="{0}" />
                            </Columns>
                            <AjaxConfiguration UpdatePanelId="UpdatePanel1" />
                        </bx:BXGridView>
                        <bx:BXMultiActionMenuToolbar ID="BackupsMultiActionMenuToolbar" runat="server" DefaultActive="true">
                            <Items>
                                <bx:BXMamImageButton ID="BXMamImageButton1" runat="server" ShowConfirmDialog="true"
                                    ConfirmDialogText="<%$ LocRaw:ActionConfirmDialogText.Delete %>" ConfirmDialogTextAll="<%$ LocRaw:ActionConfirmDialogTextAll.Delete %>"
                                    EnabledCssClass="context-button icon delete" DisabledCssClass="context-button icon delete-dis"
                                    CommandName="delete" />
                            </Items>
                        </bx:BXMultiActionMenuToolbar>
                    </ContentTemplate>
                </asp:UpdatePanel>
            </td>
        </tr>
        <tr>
            <td>
                <div style="background-color: #fefdea; border-color: #e3e2cf; border-width: 1px;
                    border-style: solid; padding: 10px">
                    <span>Для маски исключения действуют следующие правила:</span>
                    <ul style="font-size: 11px;">
                        <li>шаблон маски может содержать символы "*", которые соответствуют любому количеству
                            любых символов в имени файла или папки; </li>
                        <li>если в начале стоит косая черта ("/" или "\"), путь считается от корня сайта;
                        </li>
                        <li>в противном случае шаблон применяется к каждому файлу или папке;</li>
                    </ul>
                    <span>Примеры шаблонов:</span>
                    <ul style="font-size: 11px;">
                        <li>/content/photo - исключить целиком папку /content/photo;</li>
                        <li>*.zip - исключить файлы с расширением "zip"; </li>
                        <li>.access.php - исключить все файлы ".access.php";</li>
                        <li>/files/download/*.zip - исключить файлы с расширением "zip" в директории /files/download;</li>
                        <li>/files/d*/*.ht* - исключить файлы из директорий, начинающихся на "/files/d" с расширениями,
                            начинающимися на "ht".</li>
                    </ul>
                </div>
            </td>
        </tr>
    </table>
</asp:Content>