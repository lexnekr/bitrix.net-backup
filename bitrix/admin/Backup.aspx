<%@ Page Language="C#" MasterPageFile="~/bitrix/admin/AdminMasterPage.master" AutoEventWireup="false"
    CodeFile="Backup.aspx.cs" Inherits="Backup" %>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <bx:BXTabControl ID="BXTabControl1" ValidationGroup="vgInnerForm" runat="server"
        OnCommand="BXTabControl1_Command" ButtonsMode="Hidden">
        <bx:BXTabControlTab ID="BXTabControlTab1" runat="server" Selected="True" Text="Модуль для резервного копирования"
            Title="<%$ LocRaw:TabTitle.Main %>">
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
                    <tr>
                        <td align="right">
                            <asp:Button ID="btnArchive" runat="server" OnClick="btnArchive_Click" Text="Архивировать"
                                OnClientClick="javascript:document.getElementById('progress').style.display='block'" />
                        </td>
                        <td align="left">
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td align="right">
                            &nbsp;
                        </td>
                        <td align="left">
                            &nbsp;
                        </td>
                    </tr>
                    <tr>
                        <td align="center" colspan="2">
                            <img id="progress" src="../images/update_progressbar.gif" style="display: none" />
                        </td>
                    </tr>
                    <tr>
                        <td align="right" colspan="2">
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
                                        OnDelete="BackupsGridView_Delete" OnPopupMenuClick="AuthBackupsGridView_PopupMenuClick"
                                        DataKeyNames="FullName">
                                        <Columns>
                                            <asp:HyperLinkField DataNavigateUrlFields="Name" DataNavigateUrlFormatString="~/backup/{0}"
                                                DataTextField="Name" HeaderText="Имя" />
                                            <asp:BoundField HeaderText="Размер файла" DataField="Size" DataFormatString="{0} Мб" />
                                            <asp:BoundField HeaderText="Изменен" DataField="Created" DataFormatString="{0}" />
                                        </Columns>
                                        <AjaxConfiguration UpdatePanelId="UpdatePanel1" />
                                    </bx:BXGridView>
                                    <bx:BXMultiActionMenuToolbar ID="BackupsMultiActionMenuToolbar" runat="server">
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
                        <td colspan="2">
                            <div style="background-color: #fefdea;border-color:#e3e2cf;border-width:1px;border-style:solid;padding:10px">
                                <span>Для маски исключения действуют следующие правила:</span>
                                <ul style="font-size:11px;">
                                    <li>шаблон маски может содержать символы "*", которые соответствуют любому количеству
                                        любых символов в имени файла или папки; </li>
                                    <li>если в начале стоит косая черта ("/" или "\"), путь считается от корня сайта;
                                    </li>
                                    <li>в противном случае шаблон применяется к каждому файлу или папке;</li>
                                </ul>
                                <span>Примеры шаблонов:</span>
                                <ul style="font-size:11px;">
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
            </div>
        </bx:BXTabControlTab>
    </bx:BXTabControl>
</asp:Content>
