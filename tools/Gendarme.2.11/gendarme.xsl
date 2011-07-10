<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" encoding="UTF-8" /> 
	<xsl:template name="print-defect-rules">
		<xsl:param name="name" />
		: <xsl:value-of select="count(//rule[@Name = $name]/target/defect)" /> defects
	</xsl:template>
	<xsl:template name="print-rules">
		<xsl:param name="type" />
			<p>
				<b><xsl:value-of select="$type" /></b>:
				<xsl:choose>
					<xsl:when test="count(rules/rule[@Type = $type]) = 0">
						<ul>
							<li>None</li>
						</ul>									
					</xsl:when>
					<xsl:otherwise>						
						<ul>
							<xsl:for-each select="rules/rule[@Type = $type]">
								<li>
									<a href="{@Uri}" target="{@Name}"><xsl:value-of select="text()" /></a>
									<xsl:call-template name="print-defect-rules">
										<xsl:with-param name="name">
											<xsl:value-of select="@Name" />
										</xsl:with-param>
									</xsl:call-template>
								</li>
							</xsl:for-each>
						</ul>						
					</xsl:otherwise>
				</xsl:choose>				
			</p>			
	</xsl:template>
	<xsl:template match="/">
		<xsl:for-each select="gendarme-output">
			<xsl:comment> saved from url=(0014)about:internet </xsl:comment>
			<xsl:text>&#10;</xsl:text>
			<html>
				<head>
					<title>Gendarme Report</title>
					<script language="javascript">
function expcol (sender, args)
{
	var el = document.getElementById (args);
	if (el.style.display == 'block') {
		sender.textContent = '[show]';
		el.style.display = 'none';
	} else {
		sender.textContent = '[hide]';
		el.style.display = 'block';
	}
}
					</script>
				</head>
				<style type="text/css">
					body {
						font: "DejaVu Sans", "Bitstream Vera Sans", Verdana, sans-serif;
					}
					h1 {
						font-size: 1.8em;
						color: #274e80;
					}
					h2 {
						font-size: 1.4em;
						color: #19194b;
					}
					h3 {
						font-size: 1.2em;
						font-weight: normal;
						color: #274e80;
					}
					p, li, b {
						font-size: 1em;
					}
					p.where, p.problem, p.found, p.solution {
						font: "Consolas", "DejaVu Sans Mono", monospace;
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;
					}
					p.severity-Critical {
						border-left: 6px solid red;
					}
					p.severity-High {
						border-left: 3px solid red;
					}
					p.severity-Medium {
						border-left: 3px solid yellow;
					}
					p.severity-Low {
						border-left: 3px solid green;
					}
					p.severity-Audit {
						border-left: 3px solid blue;
					}
					span.found {
						margin-left: 10px;
					}
					div.toc {
						background-color: #F6F6F6;
						border: 1px solid #DDDDDD;
						padding: 10px;	
						float: right;				
						width: 300px;						
					}
					a:link, a:visited {
						color: #274e80;
						text-decoration: none;
					}
					a:active, a:hover {
						color: #19194b;
						text-decoration: none;
					}
					a.go-to-rule {
						float: right;
						color: #274e80;
						font-size: 0.6em;
						font-weight: normal;
					}
				</style>
				<body>
					
					<h1>Gendarme Report</h1>
					<p>Produced on <xsl:value-of select="@date" /> UTC.</p>
					
					<div class="toc">
						<h2 align="center">Table of contents</h2>
						<p style="font-size: 10pt;">														
							<a href="#s1">1.&#160;&#160;Summary</a><br />
							<a href="#s1_1">&#160;&#160;1.1.&#160;&#160;List of assemblies searched</a><br />
							<a href="#s1_2">&#160;&#160;1.2.&#160;&#160;List of rules used</a><br />
							<a href="#s2">2.&#160;&#160;Reported defects</a><br />
              <xsl:for-each select="results/rule">
                 <a href="#{@Name}">&#160;&#160;2.<xsl:value-of select="position()" />.&#160;<xsl:value-of select="@Name" /><br />
                 </a>
              </xsl:for-each>
            </p>
					</div>
					<h1><a name="s1">Summary</a></h1>
          <p>
            <a href="http://www.mono-project.com/Gendarme">Gendarme</a> found <xsl:value-of select="count(//rule/target/defect)" /> potential defects using <xsl:value-of select="count(//rules/rule)" /> rules.
          </p>
					<p>
<h2>List of assemblies analyzed
<a style="font-size: 66%;" href="#" onclick="expcol(this,'Assemblies_block'); return false;">[show]</a></h2>
<div id="Assemblies_block" style="display:none;">
						<ul>
							<xsl:for-each select="files/file">
								<xsl:variable name="file">
									<xsl:value-of select="@Name" />
								</xsl:variable>
								
								<li><xsl:value-of select="text()" />: <xsl:value-of select="count(//target[@Assembly = $file])" /> defects</li>
							</xsl:for-each>
						</ul>
</div>
					</p>
					<p>
<h2>List of rules used
<a style="font-size: 66%;" href="#" onclick="expcol(this,'Rules_block'); return false;">[show]</a></h2>
<div id="Rules_block" style="display:none;">
						<xsl:call-template name="print-rules">						
							<xsl:with-param name="type">Assembly</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Type</xsl:with-param>
						</xsl:call-template>
						
						<xsl:call-template name="print-rules">
							<xsl:with-param name="type">Method</xsl:with-param>
						</xsl:call-template>
</div>
					</p>
					
					<h1><a name="s2">Reported Defects</a></h1>
					<p>
						<xsl:for-each select="results/rule">
							<h3>
<xsl:value-of select="position()" />.&#160;<a name="{@Name}" />
<a href="{@Uri}" target="{@Name}"><xsl:value-of select="@Name" /></a>&#160;
<a style="font-size: 66%;" href="#" onclick="expcol(this,'{@Name}_block'); return false;">[hide]</a>
							</h3>
<div id="{@Name}_block" style="display:block;">
							<b>Problem:</b>
							<p class="problem">
								<xsl:value-of select="problem" />
							</p>

							<b>Solution:</b>
							<p class="solution">
								<xsl:value-of select="solution" />
							</p>

							<b><xsl:value-of select="count(target/defect)" /> defect(s) found:</b>
							<xsl:if test="count(target) != 0">
								<xsl:for-each select="target">
                  <p class="found severity-{defect/@Severity}" title="{../@Name}">
		<b>Target:</b>&#160;<xsl:value-of select="@Name" /><br/>
                  <b>Assembly:</b>&#160;<xsl:value-of select="@Assembly" /><br/>
                    <xsl:for-each select="defect">
                      <span class="found" title="{../../@Name} on {../@Name}">
                        <br/>
                        <b>Severity:</b>&#160;<xsl:value-of select="@Severity" />&#160;
                        <b>Confidence:</b>&#160;<xsl:value-of select="@Confidence" /><br/>
                        <xsl:if test="@Location != (../@Name)">
                         <b>Location:</b>&#160;<xsl:value-of select="@Location" /><br/>
                        </xsl:if>
                       <xsl:if test="string-length(@Source) &gt; 0">
                          <b>Source:</b>&#160;<xsl:value-of select="@Source" /><br/>
                        </xsl:if>
                        <xsl:if test="string-length(.) &gt; 0">
                          <b>Details:</b>&#160;<xsl:value-of select="." /><br/>
                        </xsl:if>
                      </span>
                    </xsl:for-each>
                    <a class="go-to-rule" href="#{../@Name}">Go to <xsl:value-of select="../@Name" /> description</a>
                  </p>
                </xsl:for-each>
							</xsl:if>
</div>
						</xsl:for-each>
					</p>
				</body>
			</html>
		</xsl:for-each>
	</xsl:template>
</xsl:stylesheet>
