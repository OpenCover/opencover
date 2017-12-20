<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:template name="string-replace-all">
    <xsl:param name="text" />
    <xsl:param name="replace" />
    <xsl:param name="by" />
    <xsl:choose>
        <xsl:when test="$text = '' or $replace = ''or not($replace)" >
            <!-- Prevent this routine from hanging -->
            <xsl:value-of select="$text" />
        </xsl:when>
        <xsl:when test="contains($text, $replace)">
            <xsl:value-of select="substring-before($text,$replace)" />
            <xsl:value-of select="$by" />
            <xsl:call-template name="string-replace-all">
                <xsl:with-param name="text" select="substring-after($text,$replace)" />
                <xsl:with-param name="replace" select="$replace" />
                <xsl:with-param name="by" select="$by" />
            </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
            <xsl:value-of select="$text" />
        </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
  
  <xsl:template match="/">
    <xsl:text>&#10;</xsl:text>
    <coverage version="1">
      <xsl:text>&#10;</xsl:text>
      <xsl:for-each select="//class">
        <file>
          <xsl:variable name="newpath">
            <xsl:call-template name="string-replace-all">
              <xsl:with-param name="text" select="translate(@filename, '\', '/')" />
              <xsl:with-param name="replace" select="'/projects/opencover/main/opencover.profiler/'" />
              <xsl:with-param name="by" select="'OpenCover.Profiler/'" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:variable name="newpath2">
            <xsl:call-template name="string-replace-all">
              <xsl:with-param name="text" select="translate($newpath, '\', '/')" />
              <xsl:with-param name="replace" select="'/opencover/main/opencover.profiler/'" />
              <xsl:with-param name="by" select="'OpenCover.Profiler/'" />
            </xsl:call-template>
          </xsl:variable>
          <xsl:attribute name="path"><xsl:value-of select="$newpath2" /></xsl:attribute>
          <xsl:text>&#10;</xsl:text>
          <xsl:for-each select="lines/line">
            <lineToCover>
            <xsl:attribute name="lineNumber">
              <xsl:value-of select="@number" />
            </xsl:attribute>
            <xsl:attribute name="covered">
              <xsl:if test="@hits &gt; 0">true</xsl:if>
              <xsl:if test="@hits = 0">false</xsl:if>
            </xsl:attribute>
            </lineToCover>
            <xsl:text>&#10;</xsl:text>
          </xsl:for-each>
        </file>
        <xsl:text>&#10;</xsl:text>
      </xsl:for-each>
    </coverage>
  </xsl:template>  
</xsl:stylesheet>